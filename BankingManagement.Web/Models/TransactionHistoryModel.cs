namespace BankingManagement.Web.Models
{
    public class TransactionHistoryModel
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfterTransaction { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
    }
}
