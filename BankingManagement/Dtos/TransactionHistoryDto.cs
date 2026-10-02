namespace BankingManagement.Dtos
{
    public class TransactionHistoryDto
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfterTransaction { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
    }
}
