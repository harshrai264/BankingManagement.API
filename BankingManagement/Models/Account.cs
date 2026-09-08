using System.ComponentModel.DataAnnotations;

namespace BankingManagement.Models
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }
        public long AccountNumber { get; set; }
        public int CustomerId { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        // Navigation property to the Customer entity
        public Customer? Customer { get; set; }  // this is used to create one to one relationship between customer and account
    }
}
