using System.ComponentModel.DataAnnotations;

namespace BankingManagement.Dtos
{
    public class CreateUserDto
    {
        public string UserName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
