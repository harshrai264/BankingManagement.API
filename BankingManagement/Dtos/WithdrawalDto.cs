namespace BankingManagement.Dtos
{
    public class WithdrawalDto
    {
        public int AccountId { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }
}
