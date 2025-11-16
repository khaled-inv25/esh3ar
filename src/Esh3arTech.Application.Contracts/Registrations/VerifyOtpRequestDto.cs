using System;
using System.ComponentModel.DataAnnotations;

namespace Esh3arTech.Registrations
{
    public record VerifyOtpRequestDto
    {
        [Required]
        public Guid RegistrationRequestId { get; set; }

        [Required]
        public string OtpCode { get; set; }
    }
}
