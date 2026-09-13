using BankingManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankingManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var client = _httpClientFactory.CreateClient("BankingManagement");
            var customers = await client.GetFromJsonAsync<List<CustomerModel>>("api/Customer/GetAll");
            var accounts = await client.GetFromJsonAsync<List<AccountModel>>(
       "api/Account/GetAll");

            // Customer IDs that already have an account
            var customerIdsWithAccounts = accounts?
                .Select(a => a.CustomerId)
                .ToHashSet()
                ?? new HashSet<int>();


            // Only customers without an account
            var availableCustomers = customers?
                .Where(c => !customerIdsWithAccounts.Contains(c.CustomerId))
                .ToList();

            ViewBag.Customers = availableCustomers;
            return View();
        }

        // to post (add) the account to the API
        [HttpPost]
        public async Task<IActionResult> Add(CreateAccountModel account)
        {
            if (!ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient("BankingManagement");
                var customers = await client.GetFromJsonAsync<List<CustomerModel>>("api/Customer/GetAll");
                ViewBag.Customers = customers;
                return View(account);
            }
            var apiClient = _httpClientFactory.CreateClient("BankingManagement");
            var response = await apiClient.PostAsJsonAsync("api/Account/Add", account);

            if (response.IsSuccessStatusCode) {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", $"API Error:{response.StatusCode}");

            //reload if api returns an error

            var customerlist = await apiClient.GetFromJsonAsync<List<CustomerModel>>("api/Customer/GetAll");
            ViewBag.Customers = customerlist;
            return View(account);
        }

        // Index page to show all the accounts

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("BankingManagement");
            var accounts = await client.GetFromJsonAsync<List<AccountModel>>("api/Account/GetAll");
            return View(accounts);
        }
    }
}
