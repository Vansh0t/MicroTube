using System.Text;
using System.Text.RegularExpressions;

namespace MicroTube.Services.Validation
{
    public class DefaultPasswordValidator : IPasswordValidator
    {
        private const int MIN_LENGTH = 6;
        private const int MAX_LENGTH = 32;
        private const string DIGIT_REGEX = @"\d";
        private const string LETTER_REGEX = @"[A-Za-z]";
        public IServiceResult Validate(string password)
        {
            if(string.IsNullOrWhiteSpace(password))
            {
                return ServiceResult.Fail(400, "No password provided");
            }
            StringBuilder errors = new StringBuilder();
            if(password.Length < MIN_LENGTH)
            {
                errors.AppendLine($"Minimum password length must be {MIN_LENGTH} characters.");
            }
            else if (password.Length > MAX_LENGTH)
            {
                errors.AppendLine($"Maximum password length must be {MAX_LENGTH} characters.");
            }
            if (!Regex.IsMatch(password, DIGIT_REGEX))
            {
                errors.AppendLine("Password must contain at least 1 digit.");
            }
            if (!Regex.IsMatch(password, LETTER_REGEX))
            {
                errors.AppendLine("Password must contain at least 1 letter.");
            }
            if (errors.Length <= 0)
                return ServiceResult.Success();
            return ServiceResult.Fail(400, errors.ToString());
        }
    }
}
