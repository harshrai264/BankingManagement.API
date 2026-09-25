using System.ComponentModel.DataAnnotations;

namespace BankingManagement.Web.Models
{
    public class CreateUser
    {
        public string UserName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
