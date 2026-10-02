using BankingManagement.Dtos;
using BankingManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IAssistantService _assistantService;

        public ChatController(IAssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        [HttpPost("Ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Query message is required.");
            }

            try
            {
                var response = await _assistantService.ProcessQueryAsync(request.Message);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Assistant processing error: {ex.Message}");
            }
        }
    }
}
