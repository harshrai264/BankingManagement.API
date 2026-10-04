namespace BankingManagement.Web.Models
{
    public class EmailLogViewModel
    {
        public int Id { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int AccountId { get; set; }
        public long AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; }
        public string? ResendEmailId { get; set; }
    }
}
