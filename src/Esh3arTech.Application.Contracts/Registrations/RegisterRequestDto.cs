using Esh3arTech.RegistrationRequests;
using System.ComponentModel.DataAnnotations;

namespace Esh3arTech.Registrations
{
    public record class RegisterRequestDto
    {
        [Required]
        public string MobileNumber { get; set; }

        [Required]
        public OS OS { get; set; }

        public bool NoSms { get; set; }

        [Required]
        public string HowToSendOtp { get; set; } = "email";
    }
}
