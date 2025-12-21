# Architecture Overview: Dual-Mode Messaging System

## System Modes

The enhanced messaging system supports two distinct messaging modes:

### Mode 1: One-Way Read-Only Messages (Existing - Keep & Enhance)
**Purpose:** Business-to-consumer read-only messages, alerts, announcements

**Flow:**
```
Business User → API → Message Queue → Worker → Database
                                              ↓
                                         SignalR (if online) → Mobile User(s)
                                              ↓
                                         Queued (if offline) → Delivered when online
```

**Characteristics:**
- Sender: Business users with message sending permissions
- Recipients: One or many mobile users
- No conversation context
- Read-only (no reply capability)
- Delivered in real-time if online, queued if offline
- Examples: Announcements, alerts, updates, broadcasts

### Mode 2: 1-to-1 Chat (New Feature)
**Purpose:** Bidirectional conversations between business users and mobile users

**Flow:**
```
Mobile User → API → Message Queue → Worker → Database (with Conversation)
                                           ↓
                                      SignalR/Push → Business User
                                           ↓
Mobile User ← SignalR ← Worker ← Queue ← API ← Business User (Reply)
```

**Characteristics:**
- Participants: One business user and one mobile user
- Conversation context required
- Bidirectional communication (both can send and receive)
- Examples: Customer support chats, sales inquiries, account assistance

---

## Unified Message Entity

```csharp
public class Message : FullAuditedEntity<Guid>
{
    // Common fields (both modes)
    public string RecipientPhoneNumber { get; set; }
    public string MessageContent { get; set; }
    public MessageContentType ContentType { get; set; }  // Text, Json, Image, Video, etc.
    public MessageStatus Status { get; set; }
    public MessageType Type { get; set; }  // NEW: Notification or Chat
    
    // One-way notification fields
    public string? Subject { get; set; }  // Used for notifications
    
    // Chat-specific fields
    public Guid? ConversationId { get; set; }  // NULL for notifications
    public string? SenderPhoneNumber { get; set; }  // NULL for notifications (use CreatorId)
    
    // Media support (both modes)
    public List<MediaAttachment> Attachments { get; set; }
}

public enum MessageType : byte
{
    OneWay = 0,  // Read-only from business to mobile users
    Chat = 1     // Bidirectional between business user and mobile user
}
```

---

## API Endpoints

### One-Way Read-Only Messages (Existing - Enhanced)
```
POST   /api/messages/send-one-way                // Send read-only message to single user
POST   /api/messages/broadcast                   // Send read-only message to multiple users
GET    /api/messages/one-way                     // Get my received one-way messages
```

### 1-to-1 Chat (New)
```
POST   /api/conversations                        // Start new conversation
GET    /api/conversations                        // List my conversations
GET    /api/conversations/{id}/messages          // Get conversation messages
POST   /api/conversations/{id}/messages          // Send message in conversation
PUT    /api/conversations/{id}/mark-as-read      // Mark conversation as read
```

### Media (New - Both Modes)
```
POST   /api/media/upload                         // Upload media file
GET    /api/media/{id}/url                       // Get signed URL for media
```

---

## Data Model

### Existing Entities (Keep)
- `Message` - Enhanced with Type and ConversationId
- `MobileUser` - No changes needed
- `MessageStatus` enum - No changes needed
- `MessageContentType` enum - Add Image, Video, Audio, Document

### New Entities

#### Conversation
```csharp
public class Conversation : FullAuditedEntity<Guid>
{
    public Guid BusinessUserId { get; set; }           // Business user (IdentityUser)
    public string MobileUserPhoneNumber { get; set; }  // Mobile user
    public Guid? LastMessageId { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCountBusiness { get; set; }       // Unread count for business user
    public int UnreadCountMobile { get; set; }         // Unread count for mobile user
}
```

#### MediaAttachment
```csharp
public class MediaAttachment : Entity<Guid>
{
    public Guid MessageId { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public string BlobStorageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public MediaType Type { get; set; }  // Image, Video, Audio, Document
}
```

#### DeviceToken (for Push Notifications)
```csharp
public class DeviceToken : Entity<Guid>
{
    public string MobileNumber { get; set; }
    public string Token { get; set; }
    public DevicePlatform Platform { get; set; }  // Android, iOS
    public bool IsActive { get; set; }
    public DateTime LastUsedAt { get; set; }
}
```

---

## Message Routing Logic

```csharp
public async Task RouteMessage(Message message)
{
    if (message.Type == MessageType.OneWay)
    {
        // One-way read-only message routing
        await RouteOneWayMessage(message);
    }
    else if (message.Type == MessageType.Chat)
    {
        // Chat message routing
        await RouteChatMessage(message);
    }
}

private async Task RouteOneWayMessage(Message message)
{
    // Existing logic - no conversation needed
    // 1. Check if recipient is online
    // 2. Send via SignalR if online (real-time delivery)
    // 3. If offline, store as pending
    // 4. When user comes online, deliver queued messages
    // 5. Optionally send push notification if offline
}

private async Task RouteChatMessage(Message message)
{
    // New logic - requires conversation
    // 1. Validate conversation exists
    // 2. Update conversation last message
    // 3. Increment unread count for recipient
    // 4. Check if recipient is online
    // 5. Send via SignalR if online
    // 6. Send push notification if offline
    // 7. Store in conversation history
}
```

---

## SignalR Hub Methods

### Existing (Keep)
```csharp
// Server → Client
ReceiveMessage(messageJson)           // Receive notification
ReceivePendingMessages(messagesJson)  // Receive queued notifications

// Client → Server
AcknowledgeMessage(messageId)         // Mark notification as delivered
```

### New (Chat)
```csharp
// Server → Client
ReceiveChatMessage(messageJson)       // Receive chat message
ConversationUpdated(conversationJson) // Conversation metadata changed
TypingIndicator(conversationId, isTyping)  // Other user is typing

// Client → Server
SendChatMessage(conversationId, content, attachments)  // Send chat message
MarkConversationAsRead(conversationId)                 // Mark all as read
SendTypingIndicator(conversationId, isTyping)          // Notify typing status
```

---

## Permission Model

### One-Way Read-Only Messages
- **Sender:** Requires `Esh3arTechPermissions.Esh3arSendMessages` permission
- **Recipient:** Any verified mobile user
- **Authorization:** Business users only
- **Delivery:** Real-time if online, queued if offline

### 1-to-1 Chat
- **Participants:** One business user and one mobile user
- **Business User:** Requires chat permission
- **Mobile User:** Must be verified
- **Authorization:** Business user must have chat permissions, mobile user must be verified
- **Privacy:** Each user can only see their own conversations

---

## Benefits of Dual-Mode Design

✅ **Backward Compatibility:** Existing one-way messaging system continues to work
✅ **Clear Separation:** Read-only announcements vs. interactive support chats
✅ **Flexible Permissions:** Different authorization rules per mode
✅ **Optimized Queries:** Can filter by MessageType for better performance
✅ **Customer Support:** Business users can now have conversations with mobile users
✅ **Future Extensibility:** Easy to add more message types (e.g., Group Chat, Internal Business Chat)

---

## Migration Strategy

### Phase 1: Add New Fields (Non-Breaking)
1. Add `MessageType` enum with default value `OneWay`
2. Add nullable `ConversationId` to Message
3. Add nullable `SenderPhoneNumber` to Message
4. Existing messages automatically become `OneWay` type (read-only)

### Phase 2: Create New Entities
1. Create `Conversation` table
2. Create `MediaAttachment` table
3. Create `DeviceToken` table

### Phase 3: Implement Chat Features
1. Add conversation management APIs
2. Update SignalR hub with chat methods
3. Implement chat message routing
4. Add media upload functionality

### Phase 4: Enhance One-Way Messages
1. Add media support to one-way messages
2. Implement push notifications for both modes
3. Add retry logic for both modes
4. Ensure real-time delivery for online users
5. Ensure queued delivery for offline users

**Result:** Zero downtime, existing one-way messaging preserved and enhanced, new chat features added incrementally
