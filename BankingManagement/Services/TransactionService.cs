using BankingManagement.Data;
using BankingManagement.Dtos;
using BankingManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.NativeInterop;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BankingManagement.Services
{
    public interface ITransactionService
    {
        Task<bool> Deposit(DepositDto depositDto);
        Task<bool> Withdraw(WithdrawalDto withdrawalDto);

        Task<bool> Transfer(TransferDto transferDto);
        Task<List<TransactionHistoryDto>> TransactionHistory(int accountId);
    }

    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _context;

        public TransactionService(AppDbContext context)
        {
            _context = context;
        }
        // deposit
        public async Task<bool> Deposit(DepositDto depositDto)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == depositDto.AccountId);

            if (account == null)
            {
                return false;
            }
            if (account.Status != "Active")
            {
                throw new Exception("Account is inactive. Deposit is not allowed.");
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

            if (withdrawalDto.Amount <= 0)
            {
                throw new Exception("Withdrawal amount must be greater than zero");
            }

            if (account.Status != "Active")
            {
                throw new Exception("Account is inactive. Withdrawal is not allowed.");
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
                Description = withdrawalDto.Description
            };

            _context.Transactions.Add(transction);
            await _context.SaveChangesAsync();
            return true;

        }

        // transfer from one account to other

        public async Task<bool> Transfer(TransferDto transferDto)
       
        {
            var FromAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == transferDto.FromAccountId);

            var ToAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == transferDto.ToAccountId);

            if (FromAccount == null || ToAccount == null)
            {
                return false;
            }

            if (FromAccount.Status != "Active")
            {
                throw new Exception("Sender account is inactive. Transfer is not allowed.");
            }

            if (ToAccount.Status != "Active")
            {
                throw new Exception("Receiver account is inactive. Transfer is not allowed.");
            }

            //amount must be greater then 0
            if (transferDto.Amount <= 0)
            {
                throw new Exception("Transfer amount must be greater than zero");
            }
            // to check for same amount
            if (transferDto.FromAccountId == transferDto.ToAccountId)
            {
                throw new Exception("Cannot transfer money to the same account");
            }

            //check the sender has sufficient balace
            if (FromAccount.Balance < transferDto.Amount)
            {
                throw new Exception("Insufficent balance");
            }

            //balance after transer 
            FromAccount.Balance = FromAccount.Balance - transferDto.Amount;

            var FromTransction = new Transaction
            {
                AccountId = FromAccount.AccountId,
                TransactionType = "Withdraw",
                Amount = transferDto.Amount,
                BalanceAfterTransaction = FromAccount.Balance,
                TransactionDate = DateTime.UtcNow,
                Description = transferDto.Description
            };

            ToAccount.Balance = ToAccount.Balance + transferDto.Amount;

            var ToTransction = new Transaction
            {
                AccountId = ToAccount.AccountId,
                TransactionType = "Deposit",
                Amount = transferDto.Amount,
                BalanceAfterTransaction = ToAccount.Balance,
                TransactionDate = DateTime.UtcNow,
                Description = transferDto.Description
            };

            _context.Transactions.Add(FromTransction);
            _context.Transactions.Add(ToTransction);

            await _context.SaveChangesAsync();
            return true;
        }

        //Transction history for account

        public async Task<List<TransactionHistoryDto>> TransactionHistory(int accountId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
            
            if (account == null)
            {
                throw new Exception($"Account with ID {accountId} not found");
            }

            // to find transction

            var transactions = await _context.Transactions.Where(t => t.AccountId == accountId).OrderByDescending(t => t.TransactionDate).ToListAsync();

            var transctionHistory = transactions.Select(t => new TransactionHistoryDto
            {
                TransactionId = t.TransactionId,
                TransactionType = t.TransactionType,
                Amount = t.Amount,
                BalanceAfterTransaction = t.BalanceAfterTransaction,
                TransactionDate = t.TransactionDate,
                Description = t.Description
            }).ToList();

            return transctionHistory;
        }
    }
}
