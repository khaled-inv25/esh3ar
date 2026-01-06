using System;

namespace Esh3arTech.Messages.Buffer
{
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
}
