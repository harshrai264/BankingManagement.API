namespace BankingManagement.Web.Models
{
    public class AccountSelectionModel
    {
        public int AccountId { get; set; }

        public long? AccountNumber { get; set; }

        public string? CustomerName { get; set; }

        public decimal Balance { get; set; }

        public string? Status { get; set; }
    }
}
