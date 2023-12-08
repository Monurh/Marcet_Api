using Marcet_DB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marcet_DB.Sevice
{
    public class OrderService : IOrderService
    {
        private readonly ICartService _cartService; // Предполагается, что у вас есть сервис корзины

        public OrderService(ICartService cartService )
        {
            _cartService = cartService;
            
        }

        public Order GetOrderDetails()
        {
            // Здесь вы можете реализовать логику получения данных для отображения в представлении оформления заказа
            // Например, извлечение товаров из корзины
            var cartItems = _cartService.GetCartItems();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderDateTime = DateTime.Now,
                OrderTotal = cartItems.Sum(item => item.Total),
                OrderStatus = "Pending", // Вы можете установить статус по умолчанию
                CustomerInformation = "Здесь могут быть данные о клиенте", // Замените на фактические данные
            };

            return order;
        }

        public void PlaceOrder()
        {
            // Здесь вы реализуете логику размещения заказа, например, создание заказа в базе данных
            // и очистка корзины
            var cartItems = _cartService.GetCartItems();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderDateTime = DateTime.Now,
                OrderTotal = cartItems.Sum(item => item.Total),
                OrderStatus = "Placed",
                CustomerInformation = "Здесь могут быть данные о клиенте", // Замените на фактические данные
            };


            // Очистка корзины после размещения заказа
            _cartService.ClearCart();
        }
    }
}
