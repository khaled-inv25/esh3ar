using System.ComponentModel.DataAnnotations;

namespace Esh3arTech.Registrations
{
    public record ResendOtpRequestDto
    {
        [Required]
        public string MobileNumber { get; set; }
    }
}
