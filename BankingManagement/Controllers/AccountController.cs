using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BankingManagement.Services;
using BankingManagement.Dtos;

namespace BankingManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddAccount([FromBody] CreateAccountDto createAccountDto)
        {
            if (createAccountDto == null)
            {
                return BadRequest("Account data is required.");
            }

            var account = await _accountService.AddAccountAsync(createAccountDto.CustomerId, createAccountDto);
            return Ok(account);
        }

        [HttpGet("GetById/{accountId}")]
        public async Task<IActionResult> GetAccount(int accountId)
        {
            var result = await _accountService.GetAccountAsync(accountId);
            if(result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var result = await _accountService.GetAllAccountasync();
            return Ok(result);
        }


        [HttpPut("Update/{accountId}")]
        public async Task<IActionResult> UpdateAccount(int accountId, [FromBody] UpdateAccountDto updateAccountDto)
        {
            if (updateAccountDto == null)
            {
                return BadRequest();
            }
            var result = await _accountService.UpdateAccount(accountId, updateAccountDto);
            return Ok(result);
        }
    }

}
