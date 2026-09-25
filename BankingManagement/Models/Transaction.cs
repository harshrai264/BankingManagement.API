using System.ComponentModel.DataAnnotations;

namespace BankingManagement.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        public int AccountId { get; set; }

        public string TransactionType { get; set; }

        public decimal Amount { get; set; }

        public decimal BalanceAfterTransaction { get; set; }

        public DateTime TransactionDate { get; set; }

        public string? Description { get; set; }

        public Account Account { get; set; }
    }
}
