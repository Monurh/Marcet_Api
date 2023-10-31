using Marcet_DB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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

        [HttpPost("add")]
        public async Task<ActionResult> AddUser([FromBody] Customer userData)
        {
            try
            {
                userData.CustomerId = Guid.NewGuid();
                await db.Customers.AddAsync(userData);
                await db.SaveChangesAsync();
                return CreatedAtAction(nameof(GetUser), new { id = userData.CustomerId }, userData);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при добавлении пользователя: {ex.Message}");
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
