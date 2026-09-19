using BankingManagement.Data;
using BankingManagement.Dtos;
using BankingManagement.Models;
using Microsoft.EntityFrameworkCore;


namespace BankingManagement.Services
{

    // here created an interface for the customer service to define the contract for adding a customer asynchronously.
    // The CustomerService class implements this interface and provides the actual implementation for adding a customer to the database using Entity Framework Core.
    // this can also be done by creating a separete class for the interface and implementing it in the service class, but for simplicity, we have defined the interface and implementation in the same file.
    public interface ICustomerService
    {
        Task<List<Customer>> AddCustomerAsync(List<CreateCustomerDto> customerDtos);
        Task<CustomerResponseDto> GetCustomerAsync(int customerId);
        Task<Customer> DeleteCustomerAsync(int customerId);
        Task<CustomerResponseDto> UpdateCustomerAsync(int customerId ,UpdateCustomerDto customerDto);
        Task<List<CustomerResponseDto>> GetAllCustomersAsync();
    }

    public class CustomerService: ICustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }



        //  to add the customer to the database and return the added customer object asynchronously.
        public async Task<List<Customer>> AddCustomerAsync(List<CreateCustomerDto> customerDto) {

            var customer = customerDto.Select(c=> new Customer
            {
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                Createddate = DateTime.Now
            }).ToList();

           await _context.Customers.AddRangeAsync(customer);
            await _context.SaveChangesAsync();

            return customer;

        }

        // to get the customer
        public async Task<CustomerResponseDto?> GetCustomerAsync(int customerId)
        {

            //var customer =await _context.Customers.FindAsync(customerId);

            var customer = await _context.Customers.Include(c => c.Account).FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null)
            {
                return null;
            }
            return new CustomerResponseDto
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                CreatedDate = customer.Createddate,
                Account = customer.Account == null ? null : new AccountResponseDto
                {
                    AccountId = customer.Account.AccountId,
                    AccountNumber = customer.Account.AccountNumber,
                    Balance = customer.Account.Balance,
                    CreatedDate = customer.Account.CreatedDate

                }
            };
         
        }
        // to get all the customers
        public async Task<List<CustomerResponseDto>> GetAllCustomersAsync()
        {
            var customers = await _context.Customers.Include(c => c.Account).ToListAsync();
            return customers.Select(customer => new CustomerResponseDto
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                CreatedDate = customer.Createddate,
                Account = customer.Account == null ? null : new AccountResponseDto
                {
                    AccountId = customer.Account.AccountId,
                    AccountNumber = customer.Account.AccountNumber,
                    Balance = customer.Account.Balance,
                    CreatedDate = customer.Account.CreatedDate
                }
            }).ToList();
        }


        // to delete the customer

        public async Task<Customer?> DeleteCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);

            if (customer == null)
            {
                return null;
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return customer;
        }

        // to update the customer
        public async Task<CustomerResponseDto?> UpdateCustomerAsync(int customerId, UpdateCustomerDto customerDto)
        {
            //var customer = await _context.Customers.FindAsync(customerId);

            //if (customer == null)
            //{
            //  return null;
            //}

            //customer.Name = customerDto.Name;
            //customer.Email = customerDto.Email;
            //customer.Phone = customerDto.Phone;
            //customer.Address = customerDto.Address;
            //customer.Createddate = DateTime.Now;

            //await _context.SaveChangesAsync();
            //return customer;


            var customer = await _context.Customers.Include(c => c.Account).FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null)
            {
                return null;
            }

            // Update Customer
            customer.Name = customerDto.Name;
            customer.Email = customerDto.Email;
            customer.Phone = customerDto.Phone;
            customer.Address = customerDto.Address;

            await _context.SaveChangesAsync();

            return new CustomerResponseDto
            {

                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                CreatedDate = customer.Createddate,

                Account = customer.Account == null ? null : new AccountResponseDto
                {
                    AccountId = customer.Account.AccountId,
                    AccountNumber = customer.Account.AccountNumber,
                    CustomerId = customer.Account.CustomerId,
                    Balance = customer.Account.Balance,
                    Status = customer.Account.Status,
                    CreatedDate = customer.Account.CreatedDate
                }
            };
        }





    }
}
