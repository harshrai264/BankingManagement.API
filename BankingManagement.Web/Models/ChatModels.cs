namespace BankingManagement.Web.Models
{
    public class ChatRequestModel
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ChatResponseModel
    {
        public string Reply { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public List<string> Suggestions { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
