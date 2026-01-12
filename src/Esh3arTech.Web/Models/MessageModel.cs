using Esh3arTech.Messages;
using System;

namespace Esh3arTech.Web.Models
{
    public class MessageModel
    {
        public Guid Id { get; set; }
        public Guid CreatorId { get; set; }
        public string RecipientPhoneNumber { get; set; }
        public string? MessageContent { get; set; }
        public MessageStatus Status { get; set; }
        public string From { get; set; }
        public string AccessUrl { get; set; }
        public DateTime? UrlExpiresAt { get; set; }
    }
}
