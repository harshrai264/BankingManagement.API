using System.ComponentModel.DataAnnotations;

namespace BankingManagement.Dtos
{
    public class UpdateCustomerDto
    {
        public string Name { get; set; } = string.Empty;   // string.emplty means that This property is strictly non-nullable and should never hold a null value.
        [EmailAddress]  // this is used to validate the email address format
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(10, MinimumLength = 10)] // this is used to validate the phone number length
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must contain exactly 10 digits.")]
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}
