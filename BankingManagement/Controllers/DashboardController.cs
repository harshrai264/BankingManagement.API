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
                var totalAmount = await _context.Accounts.SumAsync(a => (decimal?)a.Balance) ?? 0m;
                var totalTransactions = await _context.Transactions.CountAsync();

                var stats = new DashboardStatsDto
                {
                    TotalCustomers = totalCustomers,
                    TotalAccounts = totalAccounts,
                    TotalAmount = totalAmount,
                    TotalTransactions = totalTransactions
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
