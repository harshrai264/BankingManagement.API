namespace BankingManagement.Web.Models
{
    public class DepositModel
    {
        public int AccountId { get; set; }

        public string? AccountNumber { get; set; }

        public string? CustomerName { get; set; }

        public decimal Balance { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }
}
