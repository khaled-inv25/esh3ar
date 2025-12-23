using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Esh3arTech.Web.Helpers
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;

        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var file = value as IFormFile;

            if (file.Length > 0)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult("The uploaded file is invalid or too large, maximum file size 1Mb");
                }
            }

            return ValidationResult.Success;
        }
    }
}
