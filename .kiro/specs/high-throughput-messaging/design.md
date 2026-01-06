# Design Document

## Overview

The high-throughput messaging system enhances the existing Esh3arTech messaging platform to handle 10,000+ concurrent individual API calls from business clients. The design focuses on scaling the existing MessageAppService infrastructure through intelligent load balancing, enhanced message buffering, and rate limiting mechanisms while maintaining the current ABP Framework architecture.

The system builds upon the existing components:
- **MessageAppService**: Current application service handling SendOneWayMessageAsync calls
- **MessageBuffer**: Existing Channel-based message queuing using IMessageBuffer
- **Message Entity**: Current domain entity with Id, Status, and retry logic
- **ABP Framework**: Existing dependency injection and service patterns

## Architecture

The enhanced system maintains the existing ABP layered architecture while adding high-throughput capabilities:

### Enhanced Application Layer
- **Load Balancer**: Distributes API calls across multiple MessageAppService instances
- **Rate Limiter**: Controls concurrent API call rates per business client
- **Enhanced MessageBuffer**: Extends existing IMessageBuffer with performance monitoring

### Existing Infrastructure (Unchanged)
- **Domain Layer**: Current Message entity and business logic
- **EntityFrameworkCore Layer**: Existing repositories and DbContext
- **Web Layer**: Current API controllers and SignalR hubs

### Horizontal Scaling Strategy
- **Stateless Services**: MessageAppService instances can be scaled horizontally
- **Shared Message Buffer**: Enhanced IMessageBuffer accessible across instances
- **Load Distribution**: Intelligent request routing based on instance capacity

## Components and Interfaces

### Load Balancer (New Component)
```csharp
// New interface in Application.Contracts layer
public interface IHighThroughputLoadBalancer
{
    /// <summary>
    /// Selects the optimal MessageAppService instance based on current load
    /// </summary>
    /// <returns>MessageAppServiceInstance with lowest load and available capacity</returns>
    /// <exception cref="BusinessException">When no healthy instances are available</exception>
    Task<MessageAppServiceInstance> SelectOptimalInstanceAsync();
    
    /// <summary>
    /// Registers a new MessageAppService instance for load balancing
    /// </summary>
    /// <param name="instance">Instance configuration with endpoint and capacity</param>
    Task RegisterInstanceAsync(MessageAppServiceInstance instance);
    
    /// <summary>
    /// Removes a MessageAppService instance from load balancing
    /// </summary>
    /// <param name="instanceId">Unique identifier of the instance to remove</param>
    Task UnregisterInstanceAsync(string instanceId);
    
    /// <summary>
    /// Gets current load metrics across all registered instances
    /// </summary>
    /// <returns>Aggregated load metrics for monitoring</returns>
    Task<LoadMetrics> GetCurrentLoadAsync();
    
    /// <summary>
    /// Checks if the system can handle additional concurrent requests
    /// </summary>
    /// <param name="requestCount">Number of additional requests to check capacity for</param>
    /// <returns>True if system has capacity, false if at or near limit</returns>
    Task<bool> CanHandleAdditionalLoadAsync(int requestCount);
}

// Implementation in Application layer
public class HighThroughputLoadBalancer : IHighThroughputLoadBalancer, ITransientDependency
{
    private readonly IDistributedCache<LoadMetrics> _loadCache;
    private readonly ILogger<HighThroughputLoadBalancer> _logger;
    
    public HighThroughputLoadBalancer(
        IDistributedCache<LoadMetrics> loadCache,
        ILogger<HighThroughputLoadBalancer> logger)
    {
        _loadCache = loadCache;
        _logger = logger;
    }
    
    public async Task<MessageAppServiceInstance> SelectOptimalInstanceAsync()
    {
        var instances = await GetActiveInstancesAsync();
        var optimalInstance = instances
            .Where(i => i.IsHealthy && i.CurrentLoad < i.MaxCapacity * 0.8)
            .OrderBy(i => i.CurrentLoad)
            .FirstOrDefault();
            
        if (optimalInstance == null)
        {
            throw new BusinessException("No available instances to handle request");
        }
        
        await IncrementInstanceLoadAsync(optimalInstance.Id);
        return optimalInstance;
    }
    
    public async Task RegisterInstanceAsync(MessageAppServiceInstance instance)
    {
        var cacheKey = $"instance:{instance.Id}";
        await _loadCache.SetAsync(cacheKey, new LoadMetrics
        {
            InstanceId = instance.Id,
            CurrentLoad = 0,
            MaxCapacity = instance.MaxCapacity,
            LastHeartbeat = DateTime.UtcNow,
            IsHealthy = true
        }, TimeSpan.FromMinutes(5));
        
        _logger.LogInformation("Registered MessageAppService instance {InstanceId}", instance.Id);
    }
    
    public async Task UnregisterInstanceAsync(string instanceId)
    {
        var cacheKey = $"instance:{instanceId}";
        await _loadCache.RemoveAsync(cacheKey);
        _logger.LogInformation("Unregistered MessageAppService instance {InstanceId}", instanceId);
    }
    
    public async Task<LoadMetrics> GetCurrentLoadAsync()
    {
        var instances = await GetActiveInstancesAsync();
        return new LoadMetrics
        {
            TotalInstances = instances.Count,
            TotalCurrentLoad = instances.Sum(i => i.CurrentLoad),
            TotalMaxCapacity = instances.Sum(i => i.MaxCapacity),
            AverageLoad = instances.Any() ? instances.Average(i => i.CurrentLoad) : 0
        };
    }
    
    public async Task<bool> CanHandleAdditionalLoadAsync(int requestCount)
    {
        var currentLoad = await GetCurrentLoadAsync();
        var availableCapacity = currentLoad.TotalMaxCapacity - currentLoad.TotalCurrentLoad;
        return availableCapacity >= requestCount;
    }
    
    private async Task<List<LoadMetrics>> GetActiveInstancesAsync()
    {
        // Implementation to get all active instances from cache
        // Filter out instances with expired heartbeats
        var allKeys = await _loadCache.GetKeysAsync("instance:*");
        var instances = new List<LoadMetrics>();
        
        foreach (var key in allKeys)
        {
            var metrics = await _loadCache.GetAsync(key);
            if (metrics != null && metrics.LastHeartbeat > DateTime.UtcNow.AddMinutes(-2))
            {
                instances.Add(metrics);
            }
        }
        
        return instances;
    }
    
    private async Task IncrementInstanceLoadAsync(string instanceId)
    {
        var cacheKey = $"instance:{instanceId}";
        var metrics = await _loadCache.GetAsync(cacheKey);
        if (metrics != null)
        {
            metrics.CurrentLoad++;
            metrics.LastHeartbeat = DateTime.UtcNow;
            await _loadCache.SetAsync(cacheKey, metrics, TimeSpan.FromMinutes(5));
        }
    }
}
```

### Enhanced Message Buffer (Extends Existing)
```csharp
// Enhanced interface extending existing IMessageBuffer
public interface IHighThroughputMessageBuffer : IMessageBuffer
{
    /// <summary>
    /// Attempts to write a message to the buffer with a timeout constraint
    /// </summary>
    /// <param name="message">Message entity to queue for processing</param>
    /// <param name="timeout">Maximum time to wait for buffer space</param>
    /// <returns>True if message was queued successfully, false if timeout exceeded</returns>
    Task<bool> TryWriteAsync(Message message, TimeSpan timeout);
    
    /// <summary>
    /// Gets current buffer performance and utilization metrics
    /// </summary>
    /// <returns>BufferMetrics with depth, capacity, and utilization percentage</returns>
    Task<BufferMetrics> GetMetricsAsync();
    
    /// <summary>
    /// Checks if buffer utilization is approaching capacity threshold
    /// </summary>
    /// <param name="threshold">Utilization threshold (0.0 to 1.0, default 0.8 = 80%)</param>
    /// <returns>True if buffer utilization exceeds threshold</returns>
    Task<bool> IsNearCapacityAsync(double threshold = 0.8);
    
    /// <summary>
    /// Gets the current number of messages in the buffer
    /// </summary>
    /// <returns>Current message count in buffer</returns>
    Task<int> GetCurrentDepthAsync();
}

// Implementation extending existing MessageBuffer
public class HighThroughputMessageBuffer : IHighThroughputMessageBuffer, ISingletonDependency
{
    private readonly Channel<Message> _channel;
    private readonly ILogger<HighThroughputMessageBuffer> _logger;
    private volatile int _currentDepth;
    
    public HighThroughputMessageBuffer(ILogger<HighThroughputMessageBuffer> logger)
    {
        _logger = logger;
        var options = new BoundedChannelOptions(Esh3arTechConsts.BufferLimit)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<Message>(options);
    }
    
    public ChannelWriter<Message> Writer => _channel.Writer;
    public ChannelReader<Message> Reader => _channel.Reader;
    
    public async Task<bool> TryWriteAsync(Message message, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await _channel.Writer.WriteAsync(message, cts.Token);
            Interlocked.Increment(ref _currentDepth);
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Failed to write message {MessageId} to buffer within timeout {Timeout}", 
                message.Id, timeout);
            return false;
        }
    }
    
    public Task<BufferMetrics> GetMetricsAsync()
    {
        return Task.FromResult(new BufferMetrics
        {
            CurrentDepth = _currentDepth,
            MaxCapacity = Esh3arTechConsts.BufferLimit,
            UtilizationPercentage = (double)_currentDepth / Esh3arTechConsts.BufferLimit * 100,
            LastUpdated = DateTime.UtcNow
        });
    }
    
    public async Task<bool> IsNearCapacityAsync(double threshold = 0.8)
    {
        var metrics = await GetMetricsAsync();
        return metrics.UtilizationPercentage >= (threshold * 100);
    }
    
    public Task<int> GetCurrentDepthAsync()
    {
        return Task.FromResult(_currentDepth);
    }
}
```

### Background Message Processor (New Component)
```csharp
// Background service that processes messages from buffer
public class HighThroughputMessageProcessor : BackgroundService, ITransientDependency
{
    private readonly IHighThroughputMessageBuffer _messageBuffer;
    private readonly IMessageRepository _messageRepository;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly ILogger<HighThroughputMessageProcessor> _logger;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    
    public HighThroughputMessageProcessor(
        IHighThroughputMessageBuffer messageBuffer,
        IMessageRepository messageRepository,
        IDistributedEventBus distributedEventBus,
        ILogger<HighThroughputMessageProcessor> logger,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _messageBuffer = messageBuffer;
        _messageRepository = messageRepository;
        _distributedEventBus = distributedEventBus;
        _logger = logger;
        _unitOfWorkManager = unitOfWorkManager;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _messageBuffer.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                // Process message in background - DB persistence + RabbitMQ
                await ProcessMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message {MessageId}", message.Id);
                // Handle retry logic or move to dead letter queue
            }
        }
    }
    
    private async Task ProcessMessageAsync(Message message)
    {
        using var uow = _unitOfWorkManager.Begin();
        
        try
        {
            // Persist to database
            await _messageRepository.InsertAsync(message);
            
            // Publish to RabbitMQ for delivery
            var sendMsgEto = new SendOneWayMessageEto
            {
                Id = message.Id,
                RecipientPhoneNumber = message.RecipientPhoneNumber,
                MessageContent = message.MessageContent,
                From = "System" // Or extract from message metadata
            };
            
            await _distributedEventBus.PublishAsync(sendMsgEto);
            
            await uow.CompleteAsync();
            
            _logger.LogDebug("Successfully processed message {MessageId}", message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message {MessageId}", message.Id);
            await uow.RollbackAsync();
            throw;
        }
    }
}
```

### Rate Limiter (New Component)
```csharp
// New interface in Application.Contracts layer
public interface IRateLimiter
{
    /// <summary>
    /// Attempts to acquire permission for API requests from a business client
    /// </summary>
    /// <param name="clientId">Unique identifier of the business client</param>
    /// <param name="requestCount">Number of requests to acquire (default 1)</param>
    /// <returns>True if requests are allowed, false if rate limit exceeded</returns>
    Task<bool> TryAcquireAsync(string clientId, int requestCount = 1);
    
    /// <summary>
    /// Gets current rate limiting information for a business client
    /// </summary>
    /// <param name="clientId">Unique identifier of the business client</param>
    /// <returns>RateLimitInfo with current usage and remaining capacity</returns>
    Task<RateLimitInfo> GetClientLimitInfoAsync(string clientId);
    
    /// <summary>
    /// Applies backpressure delay to a business client to reduce load
    /// </summary>
    /// <param name="clientId">Unique identifier of the business client</param>
    /// <param name="delay">Duration to throttle the client</param>
    Task ApplyBackpressureAsync(string clientId, TimeSpan delay);
    
    /// <summary>
    /// Checks if a business client is currently being throttled
    /// </summary>
    /// <param name="clientId">Unique identifier of the business client</param>
    /// <returns>True if client is throttled, false if requests are allowed</returns>
    Task<bool> IsClientThrottledAsync(string clientId);
}

// Implementation in Application layer
public class RateLimiter : IRateLimiter, ITransientDependency
{
    private readonly IDistributedCache<ClientRateInfo> _rateLimitCache;
    private readonly ILogger<RateLimiter> _logger;
    private readonly RateLimitOptions _options;
    
    public RateLimiter(
        IDistributedCache<ClientRateInfo> rateLimitCache,
        ILogger<RateLimiter> logger,
        IOptions<RateLimitOptions> options)
    {
        _rateLimitCache = rateLimitCache;
        _logger = logger;
        _options = options.Value;
    }
    
    public async Task<bool> TryAcquireAsync(string clientId, int requestCount = 1)
    {
        var cacheKey = $"rate_limit:{clientId}";
        var clientInfo = await _rateLimitCache.GetAsync(cacheKey) ?? new ClientRateInfo
        {
            ClientId = clientId,
            RequestCount = 0,
            WindowStart = DateTime.UtcNow,
            IsThrottled = false
        };
        
        // Reset window if expired
        if (DateTime.UtcNow - clientInfo.WindowStart > TimeSpan.FromMinutes(_options.WindowMinutes))
        {
            clientInfo.RequestCount = 0;
            clientInfo.WindowStart = DateTime.UtcNow;
            clientInfo.IsThrottled = false;
        }
        
        // Check if adding requests would exceed limit
        if (clientInfo.RequestCount + requestCount > _options.MaxRequestsPerWindow)
        {
            clientInfo.IsThrottled = true;
            await _rateLimitCache.SetAsync(cacheKey, clientInfo, TimeSpan.FromMinutes(_options.WindowMinutes));
            
            _logger.LogWarning("Rate limit exceeded for client {ClientId}. Current: {Current}, Limit: {Limit}", 
                clientId, clientInfo.RequestCount, _options.MaxRequestsPerWindow);
            return false;
        }
        
        // Allow request and update count
        clientInfo.RequestCount += requestCount;
        await _rateLimitCache.SetAsync(cacheKey, clientInfo, TimeSpan.FromMinutes(_options.WindowMinutes));
        
        return true;
    }
    
    public async Task<RateLimitInfo> GetClientLimitInfoAsync(string clientId)
    {
        var cacheKey = $"rate_limit:{clientId}";
        var clientInfo = await _rateLimitCache.GetAsync(cacheKey);
        
        return new RateLimitInfo
        {
            ClientId = clientId,
            CurrentRequests = clientInfo?.RequestCount ?? 0,
            MaxRequests = _options.MaxRequestsPerWindow,
            WindowStart = clientInfo?.WindowStart ?? DateTime.UtcNow,
            IsThrottled = clientInfo?.IsThrottled ?? false,
            RemainingRequests = Math.Max(0, _options.MaxRequestsPerWindow - (clientInfo?.RequestCount ?? 0))
        };
    }
    
    public async Task ApplyBackpressureAsync(string clientId, TimeSpan delay)
    {
        var cacheKey = $"backpressure:{clientId}";
        await _rateLimitCache.SetAsync(cacheKey, DateTime.UtcNow.Add(delay), delay);
        
        _logger.LogInformation("Applied backpressure to client {ClientId} for {Delay}", clientId, delay);
    }
    
    public async Task<bool> IsClientThrottledAsync(string clientId)
    {
        var backpressureKey = $"backpressure:{clientId}";
        var backpressureUntil = await _rateLimitCache.GetAsync(backpressureKey);
        
        if (backpressureUntil.HasValue && DateTime.UtcNow < backpressureUntil.Value)
        {
            return true;
        }
        
        var limitInfo = await GetClientLimitInfoAsync(clientId);
        return limitInfo.IsThrottled;
    }
}
```

## Data Models

### Key Class Properties Summary

```csharp
// Core Load Balancing Classes
public class MessageAppServiceInstance
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Endpoint { get; set; }
    public int MaxCapacity { get; set; } = 1000;
    public int CurrentLoad { get; set; }
    public bool IsHealthy { get; set; } = true;
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public double AverageResponseTimeMs { get; set; }
    public double CpuUtilization { get; set; }
    public double MemoryUtilization { get; set; }
}

public class LoadMetrics
{
    public string InstanceId { get; set; }
    public int CurrentLoad { get; set; }
    public int MaxCapacity { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public bool IsHealthy { get; set; }
    public int TotalInstances { get; set; }
    public int TotalCurrentLoad { get; set; }
    public int TotalMaxCapacity { get; set; }
    public double AverageLoad { get; set; }
    public double SystemUtilizationPercentage => TotalMaxCapacity > 0 ? 
        (double)TotalCurrentLoad / TotalMaxCapacity * 100 : 0;
}

public class BufferMetrics
{
    public int CurrentDepth { get; set; }
    public int MaxCapacity { get; set; }
    public double UtilizationPercentage { get; set; }
    public DateTime LastUpdated { get; set; }
    public TimeSpan AverageProcessingTime { get; set; }
    public int MessagesPerMinute { get; set; }
    public int DroppedMessages { get; set; }
}

// Rate Limiting Classes
public class ClientRateInfo
{
    public string ClientId { get; set; }
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
    public bool IsThrottled { get; set; }
    public DateTime? ThrottledUntil { get; set; }
    public int RejectedRequests { get; set; }
}

public class RateLimitInfo
{
    public string ClientId { get; set; }
    public int CurrentRequests { get; set; }
    public int MaxRequests { get; set; }
    public int RemainingRequests { get; set; }
    public DateTime WindowStart { get; set; }
    public bool IsThrottled { get; set; }
    public DateTime WindowResetTime => WindowStart.AddMinutes(1);
    public TimeSpan TimeUntilReset => WindowResetTime - DateTime.UtcNow;
}

public class RateLimitOptions
{
    public int MaxRequestsPerWindow { get; set; } = 1000;
    public int WindowMinutes { get; set; } = 1;
    public bool EnableBackpressure { get; set; } = true;
    public TimeSpan BackpressureDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxBackpressureDelay { get; set; } = TimeSpan.FromMinutes(5);
    public bool EnableBurstAllowance { get; set; } = true;
    public int BurstAllowance { get; set; } = 100;
}

// API Request/Response Classes
public class SendOneWayMessageDto
{
    [Required] [Phone] public string RecipientPhoneNumber { get; set; }
    [Required] [MaxLength(4096)] public string MessageContent { get; set; }
    public List<MessageAttachmentDto> Attachments { get; set; } = new();
    public string CorrelationId { get; set; }
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class HighThroughputMessageResponse
{
    public Guid MessageId { get; set; }
    public int StatusCode { get; set; }
    public long ProcessingTimeMs { get; set; }
    public SystemLoadInfo SystemLoad { get; set; }
    public RateLimitInfo RateLimit { get; set; }
}

public class SystemLoadInfo
{
    public double UtilizationPercentage { get; set; }
    public int ActiveInstances { get; set; }
    public double BufferUtilization { get; set; }
    public TimeSpan? RecommendedDelay { get; set; }
}

public class ErrorResponse
{
    public string Code { get; set; }
    public string Message { get; set; }
    public int StatusCode { get; set; }
    public TimeSpan? RetryAfter { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}
```

### API Request/Response Models
```csharp
// Enhanced request model for high-throughput scenarios
public class SendOneWayMessageDto
{
    /// <summary>
    /// Mobile phone number of the message recipient (required)
    /// </summary>
    [Required]
    [Phone]
    public string RecipientPhoneNumber { get; set; }
    
    /// <summary>
    /// Text content of the message to send (required)
    /// </summary>
    [Required]
    [MaxLength(4096)]
    public string MessageContent { get; set; }
    
    /// <summary>
    /// Optional file attachments for the message
    /// </summary>
    public List<MessageAttachmentDto> Attachments { get; set; } = new();
    
    /// <summary>
    /// Client-provided correlation ID for tracking
    /// </summary>
    public string CorrelationId { get; set; }
    
    /// <summary>
    /// Priority level for message processing (Normal, High, Critical)
    /// </summary>
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    
    /// <summary>
    /// Optional metadata for the message
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class MessageDto
{
    /// <summary>
    /// Unique identifier of the created message
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Current status of the message (Queued, Processing, Sent, Failed)
    /// </summary>
    public MessageStatus Status { get; set; }
    
    /// <summary>
    /// Timestamp when the message was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Client-provided correlation ID for tracking
    /// </summary>
    public string CorrelationId { get; set; }
    
    /// <summary>
    /// Estimated processing time in milliseconds
    /// </summary>
    public int EstimatedProcessingTimeMs { get; set; }
}

public class HighThroughputMessageResponse
{
    /// <summary>
    /// Unique identifier of the created message
    /// </summary>
    public Guid MessageId { get; set; }
    
    /// <summary>
    /// HTTP status code of the operation
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Processing time in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }
    
    /// <summary>
    /// Current system load information
    /// </summary>
    public SystemLoadInfo SystemLoad { get; set; }
    
    /// <summary>
    /// Rate limiting information for the client
    /// </summary>
    public RateLimitInfo RateLimit { get; set; }
}

public class SystemLoadInfo
{
    /// <summary>
    /// Current system utilization percentage (0-100)
    /// </summary>
    public double UtilizationPercentage { get; set; }
    
    /// <summary>
    /// Number of active MessageAppService instances
    /// </summary>
    public int ActiveInstances { get; set; }
    
    /// <summary>
    /// Current buffer depth percentage (0-100)
    /// </summary>
    public double BufferUtilization { get; set; }
    
    /// <summary>
    /// Recommended delay before next request (if system is under load)
    /// </summary>
    public TimeSpan? RecommendedDelay { get; set; }
}

public class ErrorResponse
{
    /// <summary>
    /// Error code identifier
    /// </summary>
    public string Code { get; set; }
    
    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Recommended time to wait before retrying
    /// </summary>
    public TimeSpan? RetryAfter { get; set; }
    
    /// <summary>
    /// Additional error details
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();
}

public enum MessagePriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}
```

### Load Balancing Models
```csharp
// New models in Application.Contracts layer
public class MessageAppServiceInstance
{
    /// <summary>
    /// Unique identifier for the MessageAppService instance
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// HTTP endpoint URL for the MessageAppService instance (e.g., "https://api1.esh3ar.com")
    /// </summary>
    public string Endpoint { get; set; }
    
    /// <summary>
    /// Maximum number of concurrent requests this instance can handle
    /// </summary>
    public int MaxCapacity { get; set; } = 1000;
    
    /// <summary>
    /// Current number of active requests being processed by this instance
    /// </summary>
    public int CurrentLoad { get; set; }
    
    /// <summary>
    /// Health status of the instance (true = healthy, false = unhealthy)
    /// </summary>
    public bool IsHealthy { get; set; } = true;
    
    /// <summary>
    /// Last time this instance reported its status
    /// </summary>
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Additional metadata about the instance (version, region, etc.)
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Average response time in milliseconds for this instance
    /// </summary>
    public double AverageResponseTimeMs { get; set; }
    
    /// <summary>
    /// CPU utilization percentage (0-100)
    /// </summary>
    public double CpuUtilization { get; set; }
    
    /// <summary>
    /// Memory utilization percentage (0-100)
    /// </summary>
    public double MemoryUtilization { get; set; }
}

public class LoadMetrics
{
    /// <summary>
    /// Instance ID this metric belongs to (null for aggregated metrics)
    /// </summary>
    public string InstanceId { get; set; }
    
    /// <summary>
    /// Current active request count for this instance
    /// </summary>
    public int CurrentLoad { get; set; }
    
    /// <summary>
    /// Maximum capacity for this instance
    /// </summary>
    public int MaxCapacity { get; set; }
    
    /// <summary>
    /// Last heartbeat timestamp for this instance
    /// </summary>
    public DateTime LastHeartbeat { get; set; }
    
    /// <summary>
    /// Health status of this instance
    /// </summary>
    public bool IsHealthy { get; set; }
    
    // Aggregated metrics across all instances
    /// <summary>
    /// Total number of registered instances
    /// </summary>
    public int TotalInstances { get; set; }
    
    /// <summary>
    /// Sum of current load across all instances
    /// </summary>
    public int TotalCurrentLoad { get; set; }
    
    /// <summary>
    /// Sum of max capacity across all instances
    /// </summary>
    public int TotalMaxCapacity { get; set; }
    
    /// <summary>
    /// Average load per instance
    /// </summary>
    public double AverageLoad { get; set; }
    
    /// <summary>
    /// Overall system utilization percentage (0-100)
    /// </summary>
    public double SystemUtilizationPercentage => TotalMaxCapacity > 0 ? 
        (double)TotalCurrentLoad / TotalMaxCapacity * 100 : 0;
}

public class BufferMetrics
{
    /// <summary>
    /// Current number of messages in the buffer
    /// </summary>
    public int CurrentDepth { get; set; }
    
    /// <summary>
    /// Maximum number of messages the buffer can hold
    /// </summary>
    public int MaxCapacity { get; set; }
    
    /// <summary>
    /// Buffer utilization as a percentage (0-100)
    /// </summary>
    public double UtilizationPercentage { get; set; }
    
    /// <summary>
    /// Timestamp when these metrics were last updated
    /// </summary>
    public DateTime LastUpdated { get; set; }
    
    /// <summary>
    /// Average time messages spend in the buffer before processing
    /// </summary>
    public TimeSpan AverageProcessingTime { get; set; }
    
    /// <summary>
    /// Number of messages processed in the last minute
    /// </summary>
    public int MessagesPerMinute { get; set; }
    
    /// <summary>
    /// Number of messages that failed to be written due to buffer full
    /// </summary>
    public int DroppedMessages { get; set; }
}
```

### Rate Limiting Models
```csharp
public class ClientRateInfo
{
    /// <summary>
    /// Unique identifier of the business client
    /// </summary>
    public string ClientId { get; set; }
    
    /// <summary>
    /// Number of requests made in the current time window
    /// </summary>
    public int RequestCount { get; set; }
    
    /// <summary>
    /// Start time of the current rate limiting window
    /// </summary>
    public DateTime WindowStart { get; set; }
    
    /// <summary>
    /// Whether the client is currently being throttled
    /// </summary>
    public bool IsThrottled { get; set; }
    
    /// <summary>
    /// Timestamp when throttling will be lifted (if throttled)
    /// </summary>
    public DateTime? ThrottledUntil { get; set; }
    
    /// <summary>
    /// Number of requests that were rejected due to rate limiting
    /// </summary>
    public int RejectedRequests { get; set; }
}

public class RateLimitInfo
{
    /// <summary>
    /// Unique identifier of the business client
    /// </summary>
    public string ClientId { get; set; }
    
    /// <summary>
    /// Current number of requests made in this time window
    /// </summary>
    public int CurrentRequests { get; set; }
    
    /// <summary>
    /// Maximum allowed requests per time window
    /// </summary>
    public int MaxRequests { get; set; }
    
    /// <summary>
    /// Number of requests remaining in current window
    /// </summary>
    public int RemainingRequests { get; set; }
    
    /// <summary>
    /// Start time of the current rate limiting window
    /// </summary>
    public DateTime WindowStart { get; set; }
    
    /// <summary>
    /// Whether the client is currently being throttled
    /// </summary>
    public bool IsThrottled { get; set; }
    
    /// <summary>
    /// Time when the current window will reset
    /// </summary>
    public DateTime WindowResetTime => WindowStart.AddMinutes(1);
    
    /// <summary>
    /// Time remaining until window reset
    /// </summary>
    public TimeSpan TimeUntilReset => WindowResetTime - DateTime.UtcNow;
}

public class RateLimitOptions
{
    /// <summary>
    /// Maximum number of requests allowed per time window
    /// </summary>
    public int MaxRequestsPerWindow { get; set; } = 1000;
    
    /// <summary>
    /// Duration of the rate limiting window in minutes
    /// </summary>
    public int WindowMinutes { get; set; } = 1;
    
    /// <summary>
    /// Whether to enable backpressure when limits are exceeded
    /// </summary>
    public bool EnableBackpressure { get; set; } = true;
    
    /// <summary>
    /// Default delay to apply when backpressure is triggered
    /// </summary>
    public TimeSpan BackpressureDelay { get; set; } = TimeSpan.FromSeconds(1);
    
    /// <summary>
    /// Maximum backpressure delay that can be applied
    /// </summary>
    public TimeSpan MaxBackpressureDelay { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Whether to enable burst allowance above the base limit
    /// </summary>
    public bool EnableBurstAllowance { get; set; } = true;
    
    /// <summary>
    /// Additional requests allowed in burst scenarios
    /// </summary>
    public int BurstAllowance { get; set; } = 100;
}
```

## Enhanced MessageAppService Integration

### Enhanced MessageAppService Interface
```csharp
// Enhanced interface in Application.Contracts layer
public interface IMessageAppService : IApplicationService
{
    /// <summary>
    /// Standard message sending with enhanced high-throughput support
    /// </summary>
    /// <param name="input">Message details including recipient and content</param>
    /// <returns>MessageDto with tracking ID and status</returns>
    /// <exception cref="BusinessException">When rate limit exceeded or system at capacity</exception>
    Task<MessageDto> SendOneWayMessageAsync(SendOneWayMessageDto input);
    
    /// <summary>
    /// High-throughput optimized message sending for concurrent scenarios
    /// </summary>
    /// <param name="input">Message details with priority and metadata</param>
    /// <returns>HighThroughputMessageResponse with detailed system information</returns>
    /// <exception cref="BusinessException">When client is throttled or buffer full</exception>
    Task<HighThroughputMessageResponse> HighThroughputSendOneWayMessageAsync(SendOneWayMessageDto input);
    
    /// <summary>
    /// Batch message sending for multiple recipients
    /// </summary>
    /// <param name="inputs">Collection of messages to send</param>
    /// <returns>Collection of MessageDto results</returns>
    Task<List<MessageDto>> SendBatchMessagesAsync(List<SendOneWayMessageDto> inputs);
    
    /// <summary>
    /// Gets current system health and capacity information
    /// </summary>
    /// <returns>SystemLoadInfo with current utilization metrics</returns>
    Task<SystemLoadInfo> GetSystemHealthAsync();
    
    /// <summary>
    /// Gets rate limiting information for the current client
    /// </summary>
    /// <returns>RateLimitInfo with current usage and limits</returns>
    Task<RateLimitInfo> GetClientRateLimitInfoAsync();
}
```

### Modified MessageAppService
```csharp
// Enhanced existing MessageAppService in Application layer
public class MessageAppService : Esh3arTechAppService, IMessageAppService
{
    private readonly IMessageFactory _messageFactory;
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IMessageRepository _messageRepository;
    private readonly IRepository<MobileUser, Guid> _mobileUserRepository;
    private readonly IBlobService _blobService;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IHighThroughputMessageBuffer _messageBuffer; // Enhanced buffer
    private readonly IRateLimiter _rateLimiter; // New rate limiter
    private readonly IHighThroughputLoadBalancer _loadBalancer; // New load balancer
    private readonly ILogger<MessageAppService> _logger;

    public MessageAppService(
        IMessageFactory messageFactory,
        IDistributedEventBus distributedEventBus,
        IMessageRepository messageRepository,
        IRepository<MobileUser, Guid> mobileUserRepository,
        IBlobService blobService,
        IUnitOfWorkManager unitOfWorkManager,
        IHighThroughputMessageBuffer messageBuffer,
        IRateLimiter rateLimiter,
        IHighThroughputLoadBalancer loadBalancer,
        ILogger<MessageAppService> logger)
    {
        _messageFactory = messageFactory;
        _distributedEventBus = distributedEventBus;
        _messageRepository = messageRepository;
        _mobileUserRepository = mobileUserRepository;
        _blobService = blobService;
        _unitOfWorkManager = unitOfWorkManager;
        _messageBuffer = messageBuffer;
        _rateLimiter = rateLimiter;
        _loadBalancer = loadBalancer;
        _logger = logger;
    }

    [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
    public async Task<MessageDto> SendOneWayMessageAsync(SendOneWayMessageDto input)
    {
        var clientId = GetClientId(); // Extract from current user or API key
        
        // Apply rate limiting
        if (!await _rateLimiter.TryAcquireAsync(clientId))
        {
            var limitInfo = await _rateLimiter.GetClientLimitInfoAsync(clientId);
            throw new BusinessException($"Rate limit exceeded. Try again after {limitInfo.WindowStart.AddMinutes(1):HH:mm:ss}");
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Create message entity with minimal validation
            var createdMessage = await CreateMessageAsync(input);
            
            // OPTIMIZATION: Only write to buffer, let background processor handle DB persistence
            var bufferWriteSuccess = await _messageBuffer.TryWriteAsync(createdMessage, TimeSpan.FromMilliseconds(80));
            
            if (!bufferWriteSuccess)
            {
                // Apply backpressure if buffer is full
                await _rateLimiter.ApplyBackpressureAsync(clientId, TimeSpan.FromSeconds(1));
                throw new BusinessException("System is at capacity. Please retry in a moment.");
            }
            
            stopwatch.Stop();
            _logger.LogInformation("Message {MessageId} queued in {ElapsedMs}ms for client {ClientId}", 
                createdMessage.Id, stopwatch.ElapsedMilliseconds, clientId);
            
            // Return immediately with message ID - background processor will handle persistence
            return new MessageDto 
            { 
                Id = createdMessage.Id,
                Status = MessageStatus.Queued,
                CreatedAt = createdMessage.CreationTime,
                EstimatedProcessingTimeMs = 50 // Estimated background processing time
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to queue message for client {ClientId} in {ElapsedMs}ms", 
                clientId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
    
    // Enhanced ingestion method with high-throughput support
    [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
    public async Task<HighThroughputMessageResponse> HighThroughputSendOneWayMessageAsync(SendOneWayMessageDto input)
    {
        var clientId = GetClientId();
        var stopwatch = Stopwatch.StartNew();
        
        // Check if client is throttled
        if (await _rateLimiter.IsClientThrottledAsync(clientId))
        {
            throw new BusinessException("Client is currently throttled due to rate limiting");
        }
        
        // Check buffer capacity before processing
        if (await _messageBuffer.IsNearCapacityAsync(0.9))
        {
            _logger.LogWarning("Message buffer near capacity, applying backpressure to client {ClientId}", clientId);
            await _rateLimiter.ApplyBackpressureAsync(clientId, TimeSpan.FromMilliseconds(500));
        }
        
        var createdMessage = await CreateMessageAsync(input);
        
        // OPTIMIZATION: Only queue to buffer - background processor handles DB + RabbitMQ
        await _messageBuffer.Writer.WriteAsync(createdMessage);
        
        stopwatch.Stop();
        
        // Get system metrics for response
        var loadMetrics = await _loadBalancer.GetCurrentLoadAsync();
        var bufferMetrics = await _messageBuffer.GetMetricsAsync();
        var rateLimitInfo = await _rateLimiter.GetClientLimitInfoAsync(clientId);
        
        return new HighThroughputMessageResponse
        {
            MessageId = createdMessage.Id,
            StatusCode = 200,
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
            SystemLoad = new SystemLoadInfo
            {
                UtilizationPercentage = loadMetrics.SystemUtilizationPercentage,
                ActiveInstances = loadMetrics.TotalInstances,
                BufferUtilization = bufferMetrics.UtilizationPercentage,
                RecommendedDelay = bufferMetrics.UtilizationPercentage > 80 ? 
                    TimeSpan.FromMilliseconds(100) : null
            },
            RateLimit = rateLimitInfo
        };
    }
    
    [Authorize(Esh3arTechPermissions.Esh3arSendMessages)]
    public async Task<List<MessageDto>> SendBatchMessagesAsync(List<SendOneWayMessageDto> inputs)
    {
        var clientId = GetClientId();
        
        // Check rate limit for batch size
        if (!await _rateLimiter.TryAcquireAsync(clientId, inputs.Count))
        {
            throw new BusinessException($"Batch size {inputs.Count} exceeds rate limit");
        }
        
        var results = new List<MessageDto>();
        var tasks = inputs.Select(async input =>
        {
            var message = await CreateMessageAsync(input);
            await _messageBuffer.Writer.WriteAsync(message);
            await _messageRepository.InsertAsync(message);
            
            return new MessageDto 
            { 
                Id = message.Id,
                Status = message.Status,
                CreatedAt = message.CreationTime,
                CorrelationId = input.CorrelationId
            };
        });
        
        results.AddRange(await Task.WhenAll(tasks));
        return results;
    }
    
    public async Task<SystemLoadInfo> GetSystemHealthAsync()
    {
        var loadMetrics = await _loadBalancer.GetCurrentLoadAsync();
        var bufferMetrics = await _messageBuffer.GetMetricsAsync();
        
        return new SystemLoadInfo
        {
            UtilizationPercentage = loadMetrics.SystemUtilizationPercentage,
            ActiveInstances = loadMetrics.TotalInstances,
            BufferUtilization = bufferMetrics.UtilizationPercentage,
            RecommendedDelay = bufferMetrics.UtilizationPercentage > 90 ? 
                TimeSpan.FromSeconds(1) : null
        };
    }
    
    public async Task<RateLimitInfo> GetClientRateLimitInfoAsync()
    {
        var clientId = GetClientId();
        return await _rateLimiter.GetClientLimitInfoAsync(clientId);
    }
    
    private string GetClientId()
    {
        // Extract client ID from current user, API key, or request headers
        return CurrentUser.Id?.ToString() ?? 
               HttpContext.Request.Headers["X-Client-Id"].FirstOrDefault() ?? 
               "anonymous";
    }
    
    // Existing methods remain unchanged...
    private async Task<Message> CreateMessageAsync(SendOneWayMessageDto input)
    {
        var messageManager = _messageFactory.Create(MessageType.OneWay);
        var createdMessage = await messageManager.CreateMessageAsync(input.RecipientPhoneNumber, input.MessageContent);
        createdMessage.SetMessageStatusType(MessageStatus.Queued);
        return createdMessage;
    }
}
```

## Error Handling

### High-Throughput Error Scenarios
- **Buffer Overflow**: Apply backpressure and return 503 Service Unavailable
- **Rate Limit Exceeded**: Return 429 Too Many Requests with retry-after header
- **Instance Unavailable**: Load balancer redirects to healthy instances
- **Timeout Exceeded**: Return 408 Request Timeout for requests over 100ms

### Error Response Strategy
```csharp
public class HighThroughputErrorHandler
{
    public async Task<ErrorResponse> HandleBufferOverflowAsync(string clientId)
    {
        await _rateLimiter.ApplyBackpressureAsync(clientId, TimeSpan.FromSeconds(2));
        return new ErrorResponse
        {
            Code = "BUFFER_OVERFLOW",
            Message = "System at capacity, please retry",
            StatusCode = 503,
            RetryAfter = TimeSpan.FromSeconds(2)
        };
    }
    
    public async Task<ErrorResponse> HandleRateLimitExceededAsync(string clientId)
    {
        var limitInfo = await _rateLimiter.GetClientLimitInfoAsync(clientId);
        return new ErrorResponse
        {
            Code = "RATE_LIMIT_EXCEEDED",
            Message = $"Rate limit of {limitInfo.MaxRequests} requests per minute exceeded",
            StatusCode = 429,
            RetryAfter = TimeSpan.FromMinutes(1) - (DateTime.UtcNow - limitInfo.WindowStart)
        };
    }
}
```