namespace BankingManagement.Web.Models
{
    public class EditAccount
    {
        public int AccountId { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;
    }
}
