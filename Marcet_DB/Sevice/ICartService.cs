using Marcet_DB.Models;

namespace Marcet_DB.Sevice
{
    public interface ICartService
    {
        List<CartItem> GetCartItems();
        void AddToCart(int productId, string productName, decimal price, int quantity);
        void RemoveFromCart(int productId);
        void ClearCart();
    }

    public class CartService : ICartService
    {
        private List<CartItem> _cartItems;

        public CartService()
        {
            _cartItems = new List<CartItem>();
        }

        public List<CartItem> GetCartItems()
        {
            return _cartItems;
        }

        public void AddToCart(int productId, string productName, decimal price, int quantity)
        {
            var existingItem = _cartItems.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    Price = price,
                    Quantity = quantity
                };

                _cartItems.Add(newItem);
            }
        }

        public void RemoveFromCart(int productId)
        {
            var itemToRemove = _cartItems.FirstOrDefault(item => item.ProductId == productId);

            if (itemToRemove != null)
            {
                _cartItems.Remove(itemToRemove);
            }
        }

        public void ClearCart()
        {
            _cartItems.Clear();
        }
    }
}
