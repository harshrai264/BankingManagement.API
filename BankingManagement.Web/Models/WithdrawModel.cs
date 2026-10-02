using System.ComponentModel.DataAnnotations;

namespace BankingManagement.Web.Models
{
    public class WithdrawModel
    {
        [Required(ErrorMessage = "Please select an account.")]
        public int AccountId { get; set; }

        public string? AccountNumber { get; set; }

        public string? CustomerName { get; set; }

        public decimal Balance { get; set; }

        [Required(ErrorMessage = "Please enter an amount.")]
        [Range(1, double.MaxValue, ErrorMessage = "Withdrawal amount must be greater than zero.")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }
}
