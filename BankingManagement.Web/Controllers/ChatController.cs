using BankingManagement.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace BankingManagement.Web.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ChatController(IHttpClientFactory httpClientFactory)
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

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequestModel request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return Json(new ChatResponseModel
                {
                    Reply = "Please enter a valid question or prompt.",
                    Category = "Error"
                });
            }

            try
            {
                var client = await GetAuthenticatedClient();

                var response = await client.PostAsJsonAsync("api/Chat/Ask", new
                {
                    Message = request.Message
                });

                if (response.IsSuccessStatusCode)
                {
                    var chatResponse = await response.Content.ReadFromJsonAsync<ChatResponseModel>();
                    return Json(chatResponse ?? new ChatResponseModel { Reply = "Received empty response from assistant." });
                }

                var err = await response.Content.ReadAsStringAsync();
                return Json(new ChatResponseModel
                {
                    Reply = $"Could not process request (API {response.StatusCode}): {err}",
                    Category = "Error"
                });
            }
            catch (Exception ex)
            {
                return Json(new ChatResponseModel
                {
                    Reply = $"An error occurred while connecting to the assistant: {ex.Message}",
                    Category = "Error"
                });
            }
        }
    }
}
