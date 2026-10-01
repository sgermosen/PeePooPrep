using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        [StringLength(128)]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(128)]
        [RegularExpression(PasswordRules.Pattern, ErrorMessage = PasswordRules.Message)]
        public string NewPassword { get; set; }
    }
}
