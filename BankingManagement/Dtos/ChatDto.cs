namespace BankingManagement.Dtos
{
    public class ChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ChatResponseDto
    {
        public string Reply { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public object? StructuredData { get; set; }
        public List<string> Suggestions { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
