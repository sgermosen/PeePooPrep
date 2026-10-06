using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
        [Required]
        [StringLength(40, MinimumLength = 1)]
        public string DisplayName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(128)]
        [RegularExpression(PasswordRules.Pattern, ErrorMessage = PasswordRules.Message)]
        public string Password { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z0-9_.]{3,24}$", ErrorMessage = "Username must be 3-24 characters: letters, numbers, dots or underscores.")]
        public string Username { get; set; }
    }

    public static class PasswordRules
    {
        public const string Pattern = "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,}$";
        public const string Message = "Password must be at least 8 characters and include an uppercase letter, a lowercase letter and a digit";
    }
}
