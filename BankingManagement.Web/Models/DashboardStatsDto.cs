namespace BankingManagement.Web.Models
{
    public class DashboardStatsDto
    {
        public int TotalCustomers { get; set; }
        public int TotalAccounts { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalTransactions { get; set; }
    }
}
