using System.ComponentModel.DataAnnotations;

namespace BankingManagement.Models
{
    public class Customer
    {
        [Key]  // this is used to make id primary key
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;   // string.emplty means that This property is strictly non-nullable and should never hold a null value.
       // [EmailAddress]  // this is used to validate the email address format
        public string Email { get; set; } = string.Empty;
       
       // [Required]
       // [StringLength(10, MinimumLength = 10)] // this is used to validate the phone number length
        public string Phone { get; set; }
        public string Address { get; set; } 
        public DateTime Createddate { get; set; }

        public Account? Account { get; set; }  // this is used to create one to one relationship between customer and account
    }
}
