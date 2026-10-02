using BankingManagement.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace BankingManagement.Web.Controllers
{
    public class TransactionController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TransactionController (IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
            
        }

        private async Task<HttpClient> GetAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("BankingManagement");

            var authResult = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var token = authResult.Properties.GetTokenValue("access_token");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        [HttpGet]
        public async Task<IActionResult> Deposit()
        {
            var client = await GetAuthenticatedClient();

            var account = await client.GetFromJsonAsync<List<AccountSelectionModel>>(
                "api/Account/GetAll");

            // to show only active accout

            var activeAccount = account?.Where(x => x.Status == "Active").ToList() ?? new List<AccountSelectionModel>();

            ViewBag.Accounts = activeAccount;
            return View(new DepositModel());
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(DepositModel depositModel)
        {
            var client = await GetAuthenticatedClient();

            var response = await client.PostAsJsonAsync(
                "api/Transaction/Deposit",
                new
                {
                    AccountId = depositModel.AccountId,
                    Amount = depositModel.Amount,
                    Description = depositModel.Description
                }   
            );

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Deposit of ₹{depositModel.Amount:N2} completed successfully.";
                return RedirectToAction("Deposit");
            }

            var error = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                "",
                $"API Error: {response.StatusCode} - {error}"
            );

            var account = await client.GetFromJsonAsync<List<AccountSelectionModel>>(
                "api/Account/GetAll");
            var activeAccount = account?.Where(x => x.Status == "Active").ToList() ?? new List<AccountSelectionModel>();
            ViewBag.Accounts = activeAccount;

            return View(depositModel);
        }

        // antigravity

        [HttpGet]
        public async Task<IActionResult> Withdraw()
        {
            var client = await GetAuthenticatedClient();

            var account = await client.GetFromJsonAsync<List<AccountSelectionModel>>(
                "api/Account/GetAll");

            // to show only active accounts
            var activeAccount = account?.Where(x => x.Status == "Active").ToList() ?? new List<AccountSelectionModel>();

            ViewBag.Accounts = activeAccount;
            return View(new WithdrawModel());
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(WithdrawModel withdrawModel)
        {
            var client = await GetAuthenticatedClient();

            var response = await client.PostAsJsonAsync(
                "api/Transaction/Withdraw",
                new
                {
                    AccountId = withdrawModel.AccountId,
                    Amount = withdrawModel.Amount,
                    Description = withdrawModel.Description
                }
            );

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Withdrawal of ₹{withdrawModel.Amount:N2} completed successfully.";
                return RedirectToAction("Withdraw");
            }

            var error = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                "",
                !string.IsNullOrWhiteSpace(error) && !error.StartsWith("{")
                    ? error.Trim('"')
                    : $"API Error: {response.StatusCode} - {error}"
            );

            var account = await client.GetFromJsonAsync<List<AccountSelectionModel>>(
                "api/Account/GetAll");
            var activeAccount = account?.Where(x => x.Status == "Active").ToList() ?? new List<AccountSelectionModel>();
            ViewBag.Accounts = activeAccount;

            return View(withdrawModel);
        }

        [HttpGet]
        public async Task<IActionResult> Transfer()
        {
            var client = await GetAuthenticatedClient();

            var account = await client.GetFromJsonAsync<List<AccountSelectionModel>>(
                "api/Account/GetAll");

            var activeAccount = account?.Where(x => x.Status == "Active").ToList() ?? new List<AccountSelectionModel>();

            ViewBag.Accounts = activeAccount;
            return View(new TransferModel());
        }

        [HttpPost]
        public async Task<IActionResult> Transfer(TransferModel transferModel)
        {
            var client = await GetAuthenticatedClient();

            if (transferModel.FromAccountId == transferModel.ToAccountId)
            {
                ModelState.AddModelError("", "Sender and receiver accounts cannot be the same.");
                var accounts = await client.GetFromJsonAsync<List<AccountSelectionModel>>("api/Account/GetAll");
                ViewBag.Accounts = accounts?.Where(x => x.Status == "Active").ToList() ?? new List<AccountSelectionModel>();
                return View(transferModel);
            }

            var response = await client.PostAsJsonAsync(
                "api/Transaction/Transfer",
                new
                {
                    FromAccountId = transferModel.FromAccountId,
                    ToAccountId = transferModel.ToAccountId,
                    Amount = transferModel.Amount,
                    Description = transferModel.Description
                }
            );

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"Transfer of ₹{transferModel.Amount:N2} completed successfully.";
                return RedirectToAction("Transfer");
            }

            var error = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                "",
                !string.IsNullOrWhiteSpace(error) && !error.StartsWith("{")
                    ? error.Trim('"')
                    : $"API Error: {response.StatusCode} - {error}"
            );

            var account = await client.GetFromJsonAsync<List<AccountSelectionModel>>(
                "api/Account/GetAll");
            var activeAccount = account?.Where(x => x.Status == "Active").ToList() ?? new List<AccountSelectionModel>();
            ViewBag.Accounts = activeAccount;

            return View(transferModel);
        }

        [HttpGet]
        public async Task<IActionResult> History(int? accountId)
        {
            var client = await GetAuthenticatedClient();

            var accounts = await client.GetFromJsonAsync<List<AccountSelectionModel>>("api/Account/GetAll");
            var activeAccounts = accounts?.Where(x => x.Status == "Active").ToList() ?? new List<AccountSelectionModel>();

            var viewModel = new TransactionHistoryViewModel
            {
                Accounts = activeAccounts,
                SelectedAccountId = accountId
            };

            if (accountId.HasValue && accountId.Value > 0)
            {
                viewModel.SelectedAccount = activeAccounts.FirstOrDefault(a => a.AccountId == accountId.Value);

                try
                {
                    var response = await client.GetAsync($"api/Transaction/TransHistory/{accountId.Value}");
                    if (response.IsSuccessStatusCode)
                    {
                        var transactions = await response.Content.ReadFromJsonAsync<List<TransactionHistoryModel>>();
                        viewModel.Transactions = transactions ?? new List<TransactionHistoryModel>();
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = !string.IsNullOrWhiteSpace(error) && !error.StartsWith("{")
                            ? error.Trim('"')
                            : $"Failed to load transaction history (Status: {response.StatusCode})";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error loading transactions: {ex.Message}";
                }
            }

            return View(viewModel);
        }
    }
}
