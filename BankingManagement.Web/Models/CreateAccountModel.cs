using System.ComponentModel.DataAnnotations;

namespace BankingManagement.Web.Models
{
    public class CreateAccountModel
    {
        [Required(ErrorMessage = "Please select a customer.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Balance is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Balance cannot be negative.")]
        public decimal Balance { get; set; }

        [Required(ErrorMessage = "Please select account status.")]
        public string Status { get; set; } = string.Empty;
    }
}

