using BankingManagement.Dtos;
using BankingManagement.Models;
using BankingManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }
        // to add the customer
        [HttpPost("add")]
        public async Task<IActionResult> AddCustomer([FromBody] List<CreateCustomerDto> customerDto)
        {
            //if (customerDto == null)
            //{
            //    return BadRequest();
            //}

            if (customerDto == null || customerDto.Count == 0)
            {
                return BadRequest("At least one customer is required.");
            }

            var result = await _customerService.AddCustomerAsync(customerDto);
            return Ok(result);
        }

        // to get the customer by id
        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var result = await _customerService.GetCustomerAsync(id);

            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        // to delete the customer by id

        [HttpDelete("Delete/{id}")]

        public async Task<IActionResult> DeleteCustomer(int id) {

            var result = await _customerService.DeleteCustomerAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // to update the customer by id
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto customerDto)
        {
          if (customerDto == null)
            {
                return BadRequest();
            }
          var result = await _customerService.UpdateCustomerAsync(id, customerDto);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);

        }
    }
}

