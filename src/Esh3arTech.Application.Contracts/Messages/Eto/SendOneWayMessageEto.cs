using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using Volo.Abp.EventBus;

namespace Esh3arTech.Messages.Eto
{
    [EventName("Esh3arTech.Messages.SendMessageEto")]
    public class SendOneWayMessageEto : AuditedEntityDto<Guid>
    {
        [RegularExpression(@"^(77|78|70|73|71)\d{7}$")]
        public string RecipientPhoneNumber { get; set; }

        public string Subject { get; set; }

        [Required]
        public string MessageContent { get; set; }
    }
}
