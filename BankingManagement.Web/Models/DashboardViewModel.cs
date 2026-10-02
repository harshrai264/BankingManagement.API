namespace BankingManagement.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public decimal TotalAmount { get; set; }

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
