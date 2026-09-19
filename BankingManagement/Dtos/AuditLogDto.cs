namespace BankingManagement.Dtos
{
    public class AuditLogDto
    {
        public int AuditLogId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Module { get; set; } = string.Empty;

        public int? RecordId { get; set; }

        public string? UserName { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public DateTime CreatedDate { get; set; }

    }
}
