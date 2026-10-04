using BankingManagement.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace BankingManagement.Web.Controllers
{
    public class NotificationLogController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NotificationLogController> _logger;

        public NotificationLogController(IHttpClientFactory httpClientFactory, ILogger<NotificationLogController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
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
            try
            {
                var client = await GetAuthenticatedClient();
                var logs = await client.GetFromJsonAsync<List<EmailLogViewModel>>("api/EmailLog/GetAll");
                return View(logs ?? new List<EmailLogViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch email logs from API");
                ViewBag.ErrorMessage = "Unable to connect to notification service. Please ensure the API is running.";
                return View(new List<EmailLogViewModel>());
            }
        }
    }
}
