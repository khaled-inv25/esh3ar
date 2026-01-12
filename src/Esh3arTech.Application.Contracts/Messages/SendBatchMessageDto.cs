using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Esh3arTech.Messages
{
    public class SendBatchMessageDto
    {
        [Required]
        [MaxLength(500)]
        public List<SendOneWayMessageDto> BatchMessages { get; set; }
    }
}
