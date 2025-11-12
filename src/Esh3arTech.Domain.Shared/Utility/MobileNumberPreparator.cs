using System.Text.RegularExpressions;
using Volo.Abp;

namespace Esh3arTech.Utility
{
    public static partial class MobileNumberPreparator
    {
        public static string PrepareMobileNumber(string mobileNumber)
        {
            // Remove any non-digit characters
            var digitsOnly = Regex.Replace(mobileNumber, @"\D", string.Empty);
            // Ensure the number starts with '77, 78, 73, 71, 70' and is 9 digits long
            ValidateMobileNumberForm(digitsOnly);
            // Add 967 prefix
            digitsOnly = $"967{digitsOnly}";

            return digitsOnly;
        }

        private static void ValidateMobileNumberForm(string mobileNumber)
        {
            var regex = new Regex(@"^(77|78|70|73|71)\d{7}$");

            if (!regex.IsMatch(mobileNumber))
            {
                throw new BusinessException("Invalid mobile number. It must start with 77, 78, 70, 73, or 71 and be 9 digits long.");
            }
        }
    }
}
