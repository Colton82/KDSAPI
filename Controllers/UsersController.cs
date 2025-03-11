using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KDSAPI.Data;
using KDSAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KDSAPI.Controllers
{
    /// <summary>
    /// Controller for handling user authentication
    /// </summary>
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly ISecurityService _securityService;
        private readonly IUsersDAO _usersDAO;
        private readonly IConfiguration _configuration;

        public UsersController(ISecurityService securityService, IUsersDAO usersDAO, IConfiguration configuration)
        {
            _securityService = securityService;
            _usersDAO = usersDAO;
            _configuration = configuration;
        }

        /// <summary>
        /// Logs in a user and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var user = _usersDAO.GetByUsername(loginRequest.Username);

            if (user == null || !_securityService.ValidatePassword(loginRequest.Password, user.Password))
            {
                return Unauthorized("Invalid username or password.");
            }

            // Generate JWT Token
            var token = GenerateJwtToken(user.Id, user.Username);

            return Ok(new { token });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required.");
            }

            TokenBlacklist.TryAdd(token, DateTime.UtcNow.AddMinutes(60));

            return Ok("Logged out successfully.");
        }
        private static readonly ConcurrentDictionary<string, DateTime> TokenBlacklist = new();

        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("register")]
        public IActionResult Register([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.Username) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return BadRequest("Username and password are required.");
            }
            var existingUser = _usersDAO.GetByUsername(loginRequest.Username);
            if (existingUser != null)
            {
                return BadRequest("Username already exists.");
            }
            var hashedPassword = _securityService.HashPassword(loginRequest.Password);
            _usersDAO.Create(loginRequest.Username, hashedPassword);
            return Ok("User created.");
        }

        /// <summary>
        /// Generates a JWT token for the authenticated user.
        /// </summary>
        private string GenerateJwtToken(int userId, string username)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", userId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwtSettings["ExpirationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Request object for login and register requests.
        /// </summary>
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
