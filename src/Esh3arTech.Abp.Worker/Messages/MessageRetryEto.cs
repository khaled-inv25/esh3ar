using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.EventBus;

namespace Esh3arTech.Abp.Worker.Messages
{
    [EventName("Esh3arTech.Messages.SendMessageEto")]
    public class MessageRetryEto
    {
        [Required]
        public Guid Id { get; set; }

        [RegularExpression(@"^(77|78|70|73|71)\d{7}$")]
        public string RecipientPhoneNumber { get; set; }

        [CanBeNull]
        public string? MessageContent { get; set; }

        public string From { get; set; }

        public string Subject { get; set; }

        public string AccessUrl { get; set; }

        public DateTime? UrlExpiresAt { get; set; }
    }
}
