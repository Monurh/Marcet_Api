using Marcet_DB.Sevice;
using Microsoft.AspNetCore.Mvc;


namespace Marcet_DB.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            var cartItems = _cartService.GetCartItems();
            return View(cartItems);
        }

        public IActionResult AddToCart(int productId, string productName, decimal price, int quantity)
        {
            _cartService.AddToCart(productId, productName, price, quantity);
            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int productId)
        {
            _cartService.RemoveFromCart(productId);
            return RedirectToAction("Index");
        }

        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            return RedirectToAction("Index");
        }
    }
}
