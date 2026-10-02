using System.ComponentModel.DataAnnotations;

namespace BankingManagement.Web.Models
{
    public class TransferModel
    {
        [Required(ErrorMessage = "Please select the sender account.")]
        [Display(Name = "From Account")]
        public int FromAccountId { get; set; }

        [Required(ErrorMessage = "Please select the receiver account.")]
        [Display(Name = "To Account")]
        public int ToAccountId { get; set; }

        [Required(ErrorMessage = "Please enter transfer amount.")]
        [Range(1, double.MaxValue, ErrorMessage = "Transfer amount must be greater than zero.")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }
}
