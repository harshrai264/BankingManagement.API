using BankingManagement.Data;
using BankingManagement.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankingManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var totalCustomers = await _context.Customers.CountAsync();
                var totalAccounts = await _context.Accounts.CountAsync();
                var totalActiveAccounts = await _context.Accounts.CountAsync(a => a.Status == "Active");
                var totalAmount = await _context.Accounts.SumAsync(a => (decimal?)a.Balance) ?? 0m;
                var totalTransactions = await _context.Transactions.CountAsync();

                var recentTransactions = await _context.Transactions
                    .Include(t => t.Account)
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(5)
                    .Select(t => new RecentTransactionDto
                    {
                        TransactionId = t.TransactionId,
                        AccountId = t.AccountId,
                        AccountNumber = t.Account != null ? t.Account.AccountNumber : 0,
                        TransactionType = t.TransactionType,
                        Amount = t.Amount,
                        BalanceAfterTransaction = t.BalanceAfterTransaction,
                        TransactionDate = t.TransactionDate,
                        Description = t.Description ?? string.Empty
                    })
                    .ToListAsync();

                var stats = new DashboardStatsDto
                {
                    TotalCustomers = totalCustomers,
                    TotalAccounts = totalAccounts,
                    TotalActiveAccounts = totalActiveAccounts,
                    TotalAmount = totalAmount,
                    TotalTransactions = totalTransactions,
                    RecentTransactions = recentTransactions
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
