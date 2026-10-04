using BankingManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailLogController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailLogController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var logs = await _emailService.GetAllLogsAsync();
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var log = await _emailService.GetLogByIdAsync(id);
            if (log == null)
            {
                return NotFound($"Email log with ID {id} not found.");
            }
            return Ok(log);
        }
    }
}
