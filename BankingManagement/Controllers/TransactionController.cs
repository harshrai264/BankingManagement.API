using BankingManagement.Dtos;
using BankingManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankingManagement.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
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

            var result = await _transactionService.Deposit(depositDto);

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
                    return BadRequest("Withdrawal amount must be greater than zero");
                }

                var result = await _transactionService.Withdraw(withdrawalDto);
                if (!result)
                {
                    return NotFound("Account not found");
                }

                return Ok("Amount Withdrawn Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // to transfer from 1 account to other
        [HttpPost("Transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferDto transferDto)
        {
            if (transferDto == null)
            {
                return BadRequest("Transfer Details required.");
            }

            try
            {
                var result = await _transactionService.Transfer(transferDto);

                if (!result)
                {
                    return NotFound("From account or To account not found.");
                }

                return Ok("Transfer successful.");
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //transction history

        [HttpGet("TransHistory/{accountId}")]
        public async Task<IActionResult> TransctionHistory(int accountId)
        {
            try
            {
                var result = await _transactionService.TransactionHistory(accountId);

                if (result == null)
                {
                    return BadRequest("Enter the required details");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }


    }
}
