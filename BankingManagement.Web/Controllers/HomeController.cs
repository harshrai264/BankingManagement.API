using BankingManagement.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace BankingManagement.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private async Task<HttpClient> GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("BankingManagement");

            var authResult = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var token = authResult?.Properties?.GetTokenValue("access_token");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();

            try
            {
                var client = await GetAuthenticatedClient();

                // 1. Try dedicated dashboard API endpoint
                var response = await client.GetAsync("api/Dashboard/Stats");
                if (response.IsSuccessStatusCode)
                {
                    var stats = await response.Content.ReadFromJsonAsync<DashboardStatsDto>();
                    if (stats != null)
                    {
                        viewModel.TotalCustomers = stats.TotalCustomers;
                        viewModel.TotalAccounts = stats.TotalAccounts;
                        viewModel.TotalActiveAccounts = stats.TotalActiveAccounts;
                        viewModel.TotalAmount = stats.TotalAmount;
                        viewModel.TotalTransactions = stats.TotalTransactions;
                        viewModel.RecentTransactions = stats.RecentTransactions ?? new();
                        return View(viewModel);
                    }
                }

                // 2. Fallback: Fetch via existing customer & account endpoints
                var customers = await client.GetFromJsonAsync<List<CustomerModel>>("api/Customer/GetAll");
                if (customers != null)
                {
                    viewModel.TotalCustomers = customers.Count;
                }

                var accounts = await client.GetFromJsonAsync<List<AccountModel>>("api/Account/GetAll");
                if (accounts != null)
                {
                    viewModel.TotalAccounts = accounts.Count;
                    viewModel.TotalActiveAccounts = accounts.Count(a => string.Equals(a.Status, "Active", StringComparison.OrdinalIgnoreCase));
                    viewModel.TotalAmount = accounts.Sum(a => a.Balance);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}

