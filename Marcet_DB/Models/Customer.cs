using Microsoft.AspNetCore.Http;

namespace Marcet_DB.Models
{
    public class Customer
    {
        public Guid CustomerId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Address { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Password { get; set; } = "";
        public string Rolle { get; set; } = "";
    }
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

}
