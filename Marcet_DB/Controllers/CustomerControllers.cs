using Marcet_DB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Marcet_Api.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Marcet_Log.ErrorHandling;
using Marcet_Log.ILogger;

namespace Marcet_DB.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class CustomerController : ControllerBase
    {
        private readonly ILoggerService _logger;
        private readonly ApplicationContext db;
        private readonly IConfiguration _configuration;

        public CustomerController(ApplicationContext dbContext, IConfiguration configuration , ILoggerService logger)
        {
            db = dbContext;
            _configuration = configuration;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetUser(Guid id)
        {
            try
            {
                _logger.LogInformation($"Executing GetUser method for user with ID: {id}"); 
                var user = await db.Customers.FirstOrDefaultAsync(u => u.CustomerId == id);
                if (user == null)
                {
                    return NotFound("Користувач не знайдений");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while processing GetUser", ex);
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser([FromBody] Customer userData)
        {
            try
            {
                _logger.LogInformation($"Executing RegisterUser method for user with email: {userData.Email}");

                // Generate Id automatically
                userData.CustomerId = Guid.NewGuid();

                // Set default role
                userData.Rolle = "User";

                await db.Customers.AddAsync(userData);
                await db.SaveChangesAsync();

                // Generate and return JWT token upon successful registration
                var token = GenerateJwtToken(userData.Email, userData.Rolle);

                _logger.LogInformation($"RegisterUser method executed successfully for user with email: {userData.Email}");
                return CreatedAtAction(nameof(GetUser), new { id = userData.CustomerId }, new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing RegisterUser for user with email: {userData.Email}", ex);
                return BadRequest($"Помилка під час реєстрації користувача: {ex.Message}");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("edit")]
        public async Task<ActionResult> EditUser([FromBody] Customer userData)
        {
            try
            {
                _logger.LogInformation($"Executing EditUser method for user with ID: {userData.CustomerId}");

                var user = await db.Customers.FirstOrDefaultAsync(u => u.CustomerId == userData.CustomerId);
                if (user == null)
                {
                    throw new NotFoundException("Користувач не знайдений");
                }

                // Restriction on changing role only by admins
                if (User.IsInRole("Admin") || User.Identity.Name == user.Email)
                {
                    UpdateUserData(user, userData);
                    await db.SaveChangesAsync();
                    _logger.LogInformation($"EditUser method executed successfully for user with ID: {userData.CustomerId}");
                    return Ok(userData);
                }
                else
                {
                    throw new UnauthorizedException("Недостатньо прав");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing EditUser for user with ID: {userData.CustomerId}", ex);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            try
            {
                _logger.LogInformation($"Executing DeleteUser method for user with ID: {id}");

                var user = await db.Customers.FirstOrDefaultAsync(u => u.CustomerId == id);
                if (user == null)
                {
                    throw new NotFoundException("Користувач не знайдений");
                }

                db.Customers.Remove(user);
                await db.SaveChangesAsync();
                _logger.LogInformation($"DeleteUser method executed successfully for user with ID: {id}");
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing DeleteUser for user with ID: {id}", ex);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginModel loginModel)
        {
            try
            {
                _logger.LogInformation($"Executing Login method for user with email: {loginModel.Email}");

                var user = await db.Customers.FirstOrDefaultAsync(u => u.Email == loginModel.Email);
                if (user == null || user.Password != loginModel.Password)
                {
                    return BadRequest("Неверные учетные данные");
                }

                var token = GenerateJwtToken(user.Email, user.Rolle);

                _logger.LogInformation($"Login method executed successfully for user with email: {loginModel.Email}");
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing Login for user with email: {loginModel.Email}", ex);
                return StatusCode(500, "Internal Server Error");
            }
        }

        private void UpdateUserData(Customer user, Customer userData)
        {
            user.FirstName = userData.FirstName;
            user.LastName = userData.LastName;
            user.Address = userData.Address;
            user.Email = userData.Email;
            user.Phone = userData.Phone;
            user.Password = userData.Password;
            user.Rolle = userData.Rolle;
        }
        private string GenerateJwtToken(string email, string role)
        {
            var authOptions = _configuration.GetSection("AuthOptions").Get<AuthOptions>();
            var securityKey = authOptions.GetSymmetricSecurityKey();
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email), // User's email
                new Claim(ClaimTypes.Role, role)    // User's role
            };

            var token = new JwtSecurityToken(
                issuer: authOptions.Issuer,
                audience: authOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(authOptions.TokenLifetime),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();

            return tokenHandler.WriteToken(token);
        }
    }
}