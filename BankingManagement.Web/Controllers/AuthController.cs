using BankingManagement.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Principal;

namespace BankingManagement.Web.Controllers
{
    public class AuthController : Controller
    {
        public readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // to get login page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginUser loginUser)
            {
            try { 
            var client = _httpClientFactory.CreateClient("BankingManagement");

            var loginresponse = await client.PostAsJsonAsync(
                "api/user/Login",
                loginUser
            );

            if (!loginresponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid username or password"
                );

                return View(loginUser);
            }

            // Read JWT returned by API
            var response =
                await loginresponse.Content.ReadFromJsonAsync<LoginResponse>();

            if (response == null || string.IsNullOrEmpty(response.AccessToken))
            {
                ModelState.AddModelError(
                    "",
                    "Login failed. Token was not received."
                );

                return View(loginUser);
            }

            // Create claims for logged-in user
            var claims = new List<Claim>
              {
                 new Claim(ClaimTypes.Name, loginUser.UserName)
               };

            // Create identity
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            // Create principal
            var principal = new ClaimsPrincipal(identity);

            // Authentication cookie properties
            var properties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            // Store JWT inside authentication properties
            properties.StoreTokens(new[]
            {
        new AuthenticationToken
        {
            Name = "access_token",
            Value = response.AccessToken
        }
    });

             // Sign in user
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    properties
                );

                return RedirectToAction("Index", "Home");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                ModelState.AddModelError("", "Something went wrong");

                return View(loginUser);
            }

           

        }
        // to get create user page

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        // to create new user 
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUser createUser)
        {
            var client = _httpClientFactory.CreateClient("BankingManagement");

            var response = await client.PostAsJsonAsync("api/user/CreateUser", createUser);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }

            return View(createUser);
        }
    }
}
