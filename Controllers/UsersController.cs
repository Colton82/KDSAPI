using KDSAPI.Data;
using KDSAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Bcpg;

namespace KDSAPI.Controllers
{
    /// <summary>
    /// Controller for handling user data
    /// </summary>
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly ISecurityService _securityService;
        private readonly IUsersDAO _usersDAO;

        public UsersController(ISecurityService securityService, IUsersDAO usersDAO)
        {
            _securityService = securityService;
            _usersDAO = usersDAO;
        }

        /// <summary>
        /// Logs in a user with the specified credentials.
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns>OKResult{ userID }</returns>
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

            return Ok(new { userID = user.Id });
        }

        /// <summary>
        /// Registers a new user with the specified credentials.
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns>OKResult</returns>
        [HttpPost("register")]
        public IActionResult actionResult([FromBody] LoginRequest loginRequest)
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
        /// Request object for login requests.
        /// </summary>
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
