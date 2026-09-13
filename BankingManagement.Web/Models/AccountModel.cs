namespace BankingManagement.Web.Models
{
    public class AccountModel
    {
        public int AccountId { get; set; }
        public long AccountNumber { get; set; }
        public int CustomerId { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
