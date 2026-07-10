using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ReportDto
    {
        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Reason { get; set; }
    }
}
