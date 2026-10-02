using BankingManagement.Data;
using BankingManagement.Dtos;
using BankingManagement.Models;
using BankingManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
namespace BankingManagement.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IPasswordService _passwordService;
        private readonly AppDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;

        public UserController (IPasswordService passwordService, AppDbContext context, IJwtTokenService jwtTokenService)
        {
            _passwordService = passwordService;
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        // api for create user
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser( [FromBody] CreateUserDto createUserDto)
        {
            if (createUserDto == null)
            {
                return BadRequest("Enter the required details");
            }
            //check username
            var existingUser = await _context.AllUsers.FirstOrDefaultAsync(u => u.UserName == createUserDto.UserName);
            if (existingUser != null)
            {
                return BadRequest("Username already exists");
            }

            // check email
            var existingEmail = await _context.AllUsers.FirstOrDefaultAsync(u => u.Email == createUserDto.Email);
            if (existingEmail != null)
            {
                return BadRequest("Email alreay exists");
            }

            // to create Hashpassword
            var HashPass =  _passwordService.HashPassword(createUserDto.Password);

            // to create new user
            var user = new AllUser
           
            {
                UserName = createUserDto.UserName,
                Email = createUserDto.Email,
                PasswordHash = HashPass,
                Role = createUserDto.Role,
                LastLogin = null
            };

            _context.AllUsers.Add(user);

            await _context.SaveChangesAsync();

            //Response

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                LastLogin = user.LastLogin
            };

            return Ok(response);
        }

        // API for login
        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser (LoginDto loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest("Enter the required details");
            }

            if (string.IsNullOrWhiteSpace(loginDto.UserName))
            {
                return BadRequest("Plese enter Username");

            }
              // check email
              if (string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest("Please enter Password");
            }

            //find user
            var user = await _context.AllUsers.FirstOrDefaultAsync(u => u.UserName == loginDto.UserName);

            //check null
            if (user == null)
            {
                return Unauthorized("Invalid Username or password");
            }

            // check if Password is valid
            var isPassValid = _passwordService.VerifyPassword(loginDto.Password, user.PasswordHash);

            if (!isPassValid)
            {
                return Unauthorized("Invalid Username or password");
            }

            // if username and pass is correct then generate token
            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new
            {
                AccessToken=token
            });
        }
    }
}
