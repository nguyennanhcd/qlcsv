using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace QLCSV.Validation
{
    /// <summary>
    /// Validates password strength:
    /// - At least 8 characters
    /// - At least one uppercase letter
    /// - At least one lowercase letter
    /// - At least one digit
    /// - At least one special character
    /// </summary>
    public class StrongPasswordAttribute : ValidationAttribute
    {
        private const int MinLength = 8;
        
        public StrongPasswordAttribute()
        {
            ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("Mật khẩu không được để trống");
            }

            string password = value.ToString()!;

            if (password.Length < MinLength)
            {
                return new ValidationResult($"Mật khẩu phải có ít nhất {MinLength} ký tự");
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return new ValidationResult("Mật khẩu phải có ít nhất một chữ cái viết hoa");
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return new ValidationResult("Mật khẩu phải có ít nhất một chữ cái viết thường");
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return new ValidationResult("Mật khẩu phải có ít nhất một chữ số");
            }

            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""':;{}|<>[\]\\/_+=\-]"))
            {
                return new ValidationResult("Mật khẩu phải có ít nhất một ký tự đặc biệt");
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Validates URL format
    /// </summary>
    public class ValidUrlAttribute : ValidationAttribute
    {
        public ValidUrlAttribute()
        {
            ErrorMessage = "URL không hợp lệ";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // Allow null/empty for optional fields
            }

            string url = value.ToString()!;

            if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }

    /// <summary>
    /// Validates that a field is required when IsOnline is true
    /// </summary>
    public class RequiredIfOnlineAttribute : ValidationAttribute
    {
        private readonly string _isOnlinePropertyName;

        public RequiredIfOnlineAttribute(string isOnlinePropertyName)
        {
            _isOnlinePropertyName = isOnlinePropertyName;
            ErrorMessage = "Trường này là bắt buộc cho sự kiện trực tuyến";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var isOnlineProperty = validationContext.ObjectType.GetProperty(_isOnlinePropertyName);
            if (isOnlineProperty == null)
            {
                return new ValidationResult($"Property {_isOnlinePropertyName} not found");
            }

            var isOnlineValue = isOnlineProperty.GetValue(validationContext.ObjectInstance);
            
            // Check if IsOnline is true
            bool isOnline = isOnlineValue is bool boolValue && boolValue;
            
            if (isOnline && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
