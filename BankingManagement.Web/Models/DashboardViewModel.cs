namespace BankingManagement.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalAccounts { get; set; }
        public int TotalActiveAccounts { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalTransactions { get; set; }
        public List<RecentTransactionDto> RecentTransactions { get; set; } = new();

        public double ActiveAccountPercentage
        {
            get
            {
                if (TotalAccounts <= 0) return 100.0;
                return Math.Round((double)TotalActiveAccounts / TotalAccounts * 100.0, 1);
            }
        }

        public string FormattedTotalAmount
        {
            get
            {
                if (TotalAmount >= 10000000m)
                {
                    return $"₹{(TotalAmount / 10000000m):0.##}Cr";
                }
                if (TotalAmount >= 100000m)
                {
                    return $"₹{(TotalAmount / 100000m):0.##}L";
                }
                return $"₹{TotalAmount:N2}";
            }
        }
    }
}
