using BankingManagement.Data;
using BankingManagement.Dtos;
using BankingManagement.Models;


namespace BankingManagement.Services
{

    // here created an interface for the customer service to define the contract for adding a customer asynchronously.
    // The CustomerService class implements this interface and provides the actual implementation for adding a customer to the database using Entity Framework Core.
    // this can also be done by creating a separete class for the interface and implementing it in the service class, but for simplicity, we have defined the interface and implementation in the same file.
    public interface ICustomerService
    {
        Task<List<Customer>> AddCustomerAsync(List<CreateCustomerDto> customerDtos);
        Task<Customer> GetCustomerAsync(int customerId);
        Task<Customer> DeleteCustomerAsync(int customerId);
        Task<Customer> UpdateCustomerAsync(int customerId ,UpdateCustomerDto customerDto);

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
        public async Task<Customer?> GetCustomerAsync(int customerId) 
        {

        var customer =await _context.Customers.FindAsync(customerId);

            return customer;
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
        public async Task<Customer?> UpdateCustomerAsync(int customerId ,UpdateCustomerDto customerDto)
        {
            var customer = await _context.Customers.FindAsync(customerId);

            if (customer == null)
            {
              return null;
            }

            customer.Name = customerDto.Name;
            customer.Email = customerDto.Email;
            customer.Phone = customerDto.Phone;
            customer.Address = customerDto.Address;
            customer.Createddate = DateTime.Now;

            await _context.SaveChangesAsync();
            return customer;

        }




    }
}
