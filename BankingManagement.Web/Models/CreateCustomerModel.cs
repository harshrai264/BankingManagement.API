using System.ComponentModel.DataAnnotations;

namespace BankingManagement.Web.Models
{
    public class CreateCustomerModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^[0-9]{10}$",
            ErrorMessage = "Phone must contain exactly 10 digits.")]
        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
    }
}
