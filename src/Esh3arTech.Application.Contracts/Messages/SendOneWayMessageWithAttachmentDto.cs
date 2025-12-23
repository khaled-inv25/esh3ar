using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Esh3arTech.Messages
{
    public class SendOneWayMessageWithAttachmentDto
    {
        [RegularExpression(@"^(77|78|70|73|71)\d{7}$")]
        public string RecipientPhoneNumber { get; set; }

        [CanBeNull]
        public string? MessageContent { get; set; }

        [Required]
        public string Base64OrJson { get; set; }

        public ContentType Type { get; set; }

        public string Subject { get; set; }
    }
}
