using System.ComponentModel.DataAnnotations;

namespace BankingManagement.Models
{
    public class AllUser
    {
        [Key]
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime ?LastLogin { get; set; }

    }
}
