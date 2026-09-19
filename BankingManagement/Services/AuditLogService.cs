using BankingManagement.Data;
using BankingManagement.Dtos;

namespace BankingManagement.Services
{
    
    public interface IAuditLogService
    {
        //Task<AuditLogDto> Audit(AuditLogDto);
    }
    public class AuditLogService :IAuditLogService
    {
        private readonly AppDbContext _context;

        public  AuditLogService(AppDbContext context)
        {
            context = _context;
        }

        //public Task<AuditLogDto> Audit(AuditLogDto) => throw new NotImplementedException();
    }
}
