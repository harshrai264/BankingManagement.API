namespace BankingManagement.Web.Models
{
    public class RecentTransactionDto
    {
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        public long AccountNumber { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfterTransaction { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class DashboardStatsDto
    {
        public int TotalCustomers { get; set; }
        public int TotalAccounts { get; set; }
        public int TotalActiveAccounts { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalTransactions { get; set; }
        public List<RecentTransactionDto> RecentTransactions { get; set; } = new();
    }
}
