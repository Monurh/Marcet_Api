using Marcet_Api.Authentication;
using Marcet_DB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Marcet_DB.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationContext db;

        public CustomerController(ApplicationContext dbContext)
        {
            db = dbContext;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetUser(Guid id)
        {
            var user = await db.Customers.FirstOrDefaultAsync(u => u.CustomerId == id);
            if (user == null)
            {
                return NotFound("Пользователь не найден");
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
                var token = GenerateJwtToken(userData.Email);

                return CreatedAtAction(nameof(GetUser), new { id = userData.CustomerId }, new { Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при регистрации пользователя: {ex.Message}");
            }
        }

        [HttpPut("edit")]
        public async Task<ActionResult> EditUser([FromBody] Customer userData)
        {
            try
            {
                var user = await db.Customers.FirstOrDefaultAsync(u => u.CustomerId == userData.CustomerId);
                if (user == null)
                {
                    return NotFound("Пользователь не найден");
                }

                UpdateUserData(user, userData);
                await db.SaveChangesAsync();
                return Ok(userData);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при редактировании пользователя: {ex.Message}");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await db.Customers.FirstOrDefaultAsync(u => u.CustomerId == id);
                if (user == null)
                {
                    return NotFound("Пользователь не найден");
                }

                db.Customers.Remove(user);
                await db.SaveChangesAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при удалении пользователя: {ex.Message}");
            }
        }

        private string GenerateJwtToken(string email)
        {
            var securityKey = AuthOptions.GetSymmetricSecurityKey();
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
        new Claim(ClaimTypes.Name, email),
    };

            var token = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(15)), // Установите желаемое время жизни токена
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
    }
}
