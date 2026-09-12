using BankingManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankingManagement.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public CustomerController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(CreateCustomerModel customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            var client = _httpClientFactory.CreateClient("BankingManagement");

            var customers = new List<CreateCustomerModel>
    {
        customer
    };

            var response = await client.PostAsJsonAsync(
                "api/Customer/Add",
                customers
            );

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            var error = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                "",
                $"API Error: {response.StatusCode} - {error}"
            );

            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("BankingManagement");

            var customers = await client.GetFromJsonAsync<List<CustomerModel>>(
                "api/Customer/GetAll"
            );

            return View(customers ?? new List<CustomerModel>());
        }

        // to edit-> 1st get the customer by id and then show the customer details in the view to edit

        [HttpGet]
        public async Task<IActionResult> Edit(int id)

        {
            var client = _httpClientFactory.CreateClient("BankingManagement");
            var customer = await client.GetFromJsonAsync<CustomerModel>($"api/Customer/Get/{id}");

            if (customer == null)
            {
                return NotFound();
            }

            var editCustomer = new EditCustomer
            {
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address
            };

            ViewBag.customerId = id;

            return View(editCustomer);
        }

        // to edit the customer by id
        //[HttpPut("Update/{id}")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditCustomer customer)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.customerId = id;
                return View(customer);
            }
            var client = _httpClientFactory.CreateClient("BankingManagement");

            var response = await client.PutAsJsonAsync($"api/customer/Update/{id}", customer);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", $"API Error: {response.StatusCode}");
                ViewBag.customerId = id;
                return View(customer);
            }
        }

        // to delete the customer by id

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("BankingManagement");
            var response = await client.DeleteAsync($"api/customer/Delete/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            else
            {
                ModelState.AddModelError("", $"API Error: {response.StatusCode}");
                return RedirectToAction("Index");
            }
        }
    }
}
