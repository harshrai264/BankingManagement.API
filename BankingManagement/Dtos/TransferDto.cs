namespace BankingManagement.Dtos
{
    public class TransferDto
    {
        public int FromAccountId { get; set; }

        public int ToAccountId { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }
}
