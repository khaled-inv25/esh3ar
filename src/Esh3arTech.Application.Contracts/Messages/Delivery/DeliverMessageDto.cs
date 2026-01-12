using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Messages.Delivery
{
    public class DeliverMessageDto : EntityDto<Guid>
    {
        [RegularExpression(@"^(77|78|70|73|71)\d{7}$")]
        public string RecipientPhoneNumber { get; set; }
        [CanBeNull]
        public string? MessageContent { get; set; }
        public Guid CreatorId { get; set; }
        public MessageStatus Status { get; set; }
        public string AccessUrl { get; set; }
        public DateTime? UrlExpiresAt { get; set; }
    }
}
