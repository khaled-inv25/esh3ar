using JetBrains.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Esh3arTech.Messages
{
    public class SendBatchMessageDto
    {
        [Required]
        [MaxLength(500)]
        public List<BatchMessageItem> BatchMessages { get; set; }
    }

    public class BatchMessageItem
    {
        [RegularExpression(@"(77|78|70|73|71)\d{7}$")]
        public string MobileNumber { get; set; }

        [CanBeNull]
        public string? MessageContent { get; set; }

        public string Subject { get; set; }
    }
}
