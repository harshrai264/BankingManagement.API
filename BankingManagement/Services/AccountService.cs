using BankingManagement.Data;
using BankingManagement.Dtos;
using BankingManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;


namespace BankingManagement.Services
{

    // interface for the account service to define the contract for adding an account asynchronously.
    public interface IAccountService
    {
        Task<AccountResponseDto> AddAccountAsync(int customerId, CreateAccountDto createAccountDto);
        Task<AccountResponseDto?> GetAccountAsync(int accountId);
        Task<List<AccountResponseDto>> GetAllAccountasync();
        Task <AccountResponseDto> UpdateAccount (int accountId, UpdateAccountDto updateAccountDto);
    }
    public class AccountService : IAccountService
    {

        private readonly AppDbContext _context;

        public AccountService(AppDbContext context)
        {
            _context = context;

        }

        // to generate a random accountnumber with a specified length (14)

        private long GenerateAccountNumber()
        {
            string accountNumber = RandomNumberGenerator
                .GetInt32(1, 10)
                .ToString();

            for (int i = 1; i < 14; i++)
            {
                accountNumber += RandomNumberGenerator.GetInt32(0, 10);
            }

            return long.Parse(accountNumber);
        }


        // to Add the account to the customer
        public async Task<AccountResponseDto> AddAccountAsync(int customerId, CreateAccountDto createAccountDto)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                throw new Exception($"Customer with ID {customerId} not found.");
            }

            Account account = new Account();
            {
                account.CustomerId = customerId;
                account.Balance = createAccountDto.Balance;
                account.Status = createAccountDto.Status;
                account.AccountNumber = GenerateAccountNumber();
                account.CreatedDate = DateTime.UtcNow;
            }
            ;
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            // used account response dto to return the account details after adding the account to the customer 
            // because as account is set in to  customer Model and customer model is set in to account model
            // //so we can return the account details after adding the account to the customer

            return new AccountResponseDto
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                CustomerId = account.CustomerId,
                Balance = account.Balance,
                Status = account.Status,
                CreatedDate = account.CreatedDate
            };
        }
        //Get account by accountid
        public async Task<AccountResponseDto?> GetAccountAsync(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
            {
                return null;
            }
            return new AccountResponseDto
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                CustomerId = account.CustomerId,
                Balance = account.Balance,
                Status = account.Status,
                CreatedDate = account.CreatedDate
            };
        }

        //Get all account
        public async Task<List<AccountResponseDto>> GetAllAccountasync()
        {
            var result = await _context.Accounts.ToListAsync();

            return result.Select (account=> new AccountResponseDto
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                CustomerId = account.CustomerId,
                Balance = account.Balance,
                Status = account.Status,
                CreatedDate = account.CreatedDate
            }).ToList();
        }


        // to update the account details by accountid and updateaccountdto
        public async Task<AccountResponseDto> UpdateAccount(int accountId, UpdateAccountDto updateAccountDto)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
            {
                throw new Exception($"Account with ID {accountId} not found.");
            }
            account.Balance = updateAccountDto.Balance;
            account.Status = updateAccountDto.Status;
            await _context.SaveChangesAsync();

            return new AccountResponseDto
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                CustomerId = account.CustomerId,
                Balance = account.Balance,
                Status = account.Status,
                CreatedDate = account.CreatedDate
            };
        }
    }
}
