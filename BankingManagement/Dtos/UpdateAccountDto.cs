namespace BankingManagement.Dtos
{
    public class UpdateAccountDto
    {
        public decimal Balance { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
