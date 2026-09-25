using BankingManagement.Data;
using BankingManagement.Dtos;
using BankingManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace BankingManagement.Services
{
    public interface ITransactionService
    {
        Task<bool> Deposit(DepositDto depositDto);
        Task<bool> Withdraw(WithdrawalDto withdrawalDto);
    }

    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _context;

        public TransactionService (AppDbContext context)
        {
            _context = context;
        }
        // deposit
        public async Task<bool> Deposit(DepositDto depositDto)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == depositDto.AccountId);
           
            if (account== null)
            {
                return false;
            }
            if (depositDto.Amount <= 0)
            {
                throw new Exception("Deposit must be greater than zero");
            }

            // add the depoist ammoutn to total balance
            account.Balance = depositDto.Amount + account.Balance;

            var transction = new Transaction
            {
                AccountId = account.AccountId,
                TransactionType = "Deposit",
                Amount = depositDto.Amount,
                BalanceAfterTransaction = account.Balance,
                TransactionDate = DateTime.UtcNow,
                Description = depositDto.Description
            };

            _context.Transactions.Add(transction);
            await _context.SaveChangesAsync();

            return true;
        } 

        //withdraw

        public async Task<bool> Withdraw(WithdrawalDto withdrawalDto)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == withdrawalDto.AccountId);

            if (account == null)
            {
                return false;
            }
            //if (withdrawalDto.Amount <= 0)
            //{
            //    return false;
            //}

            if (withdrawalDto.Amount > account.Balance)
            {
                throw new Exception("Insufficient Balance");
            }
            account.Balance = account.Balance - withdrawalDto.Amount;

            var transction = new Transaction
            {
                AccountId = account.AccountId,
                TransactionType = "Withdraw",
                Amount = withdrawalDto.Amount,
                BalanceAfterTransaction = account.Balance,
                TransactionDate = DateTime.UtcNow,
                Description= withdrawalDto.Description
            };

            _context.Transactions.Add(transction);
            await _context.SaveChangesAsync();
            return true;

        }

    }
}
