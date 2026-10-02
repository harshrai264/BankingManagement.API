using System.ComponentModel.DataAnnotations;

namespace BankingManagement.Dtos
{
    public class TransferDto
    {
        [Required]
        public int FromAccountId { get; set; }
        [Required]
        public int ToAccountId { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }
}
