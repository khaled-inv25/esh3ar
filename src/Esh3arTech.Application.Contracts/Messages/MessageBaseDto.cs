using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Esh3arTech.Messages
{
    public class MessageBaseDto
    {
        [RegularExpression(@"^(77|78|70|73|71)\d{7}$")]
        public string RecipientPhoneNumber { get; set; }
        
        [CanBeNull]
        public string? MessageContent { get; set; }
    }
}
