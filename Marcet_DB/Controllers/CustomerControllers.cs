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

namespace Marcet_DB.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationContext db;
        private readonly IConfiguration _configuration;

        public CustomerController(ApplicationContext dbContext, IConfiguration configuration)
        {
            db = dbContext;
            _configuration = configuration;
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetUser(Guid id)
        {
            var user = await db.Customers.FirstOrDefaultAsync(u => u.CustomerId == id);
            if (user == null)
            {
                return NotFound("Користувач не знайдений");
            }

            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser([FromBody] Customer userData)
        {
            try
            {
                // Генерировать Id автоматически
                userData.CustomerId = Guid.NewGuid();

                // Установить роль по умолчанию
                userData.Rolle = "User";

                await db.Customers.AddAsync(userData);
                await db.SaveChangesAsync();

                // Генерация и возврат JWT-токена при успешной регистрации
                var token = GenerateJwtToken(userData.Email, userData.Rolle);

                return CreatedAtAction(nameof(GetUser), new { id = userData.CustomerId }, new { Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest($"Помилка під час реєстрації користувача: {ex.Message}");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("edit")]
        public async Task<ActionResult> EditUser([FromBody] Customer userData)
        {
            
            
                var user = await db.Customers.FirstOrDefaultAsync(u => u.CustomerId == userData.CustomerId);
                if (user == null)
                {
                   throw new NotFoundException("Користувач не знайдений");
                }

                // Ограничение на изменение роли только админами
                if (User.IsInRole("Admin") || User.Identity.Name == user.Email)
                {
                    UpdateUserData(user, userData);
                    await db.SaveChangesAsync();
                    return Ok(userData);
                }
                else
                {
                   throw new UnauthorizedException("Недостатньо прав");
                }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
                var user = await db.Customers.FirstOrDefaultAsync(u => u.CustomerId == id);
                if (user == null)
                {
                    throw new NotFoundException("Користувач не знайдений");
                }

                db.Customers.Remove(user);
                await db.SaveChangesAsync();
                return Ok(user);
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

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await db.Customers.FirstOrDefaultAsync(u => u.Email == loginModel.Email);
            if (user == null || user.Password != loginModel.Password)
            {
                return BadRequest("Неверные учетные данные");
            }

            var token = GenerateJwtToken(user.Email, user.Rolle);   

            return Ok(new { Token = token });
        }
    }
}

