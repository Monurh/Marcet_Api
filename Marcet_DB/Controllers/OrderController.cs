using Marcet_DB.Sevice;
using Microsoft.AspNetCore.Mvc;

namespace Marcet_DB.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        public IActionResult Checkout()
        {
            // Логика оформления заказа
            var order = _orderService.GetOrderDetails();
            return View(order);
        }

        [HttpPost]
        public IActionResult PlaceOrder()
        {
            // Логика размещения заказа
            _orderService.PlaceOrder();
            return RedirectToAction("Index", "Home"); // Перенаправление на главную страницу
        }
    }
}
