using System.ComponentModel.DataAnnotations;

namespace BankingManagement.Dtos
{
    public class CreateAccountDto
    {
        
        //public long AccountNumber { get; set; }
        public int CustomerId { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; } = string.Empty;
        
    }
}
