using BankingManagement.Dtos;
using BankingManagement.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankingManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController (ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("Deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositDto depositDto)
        {
            if (depositDto == null)
            {
                return BadRequest("Transaction details required");
            }

           var result=  await _transactionService.Deposit(depositDto);

            if (!result)
            {
                return NotFound("Account not found");
            }


            return Ok("Amount deposited Successfully");
        }

        [HttpPost("Withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawalDto withdrawalDto)
        {
            try
            {
                if (withdrawalDto == null)
                {
                    return BadRequest("Transaction details required");
                }

                if (withdrawalDto.Amount <= 0)
                {
                    return BadRequest("Withdrawa amount must be greater than zero");
                }

                var result = await _transactionService.Withdraw(withdrawalDto);
                if (!result)
                {
                    return NotFound("Account not found");
                }

                return Ok("Amount Withdraw Successfully");
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
