using Marcet_DB.Models;

namespace Marcet_DB.Sevice
{
    public interface IOrderService
    {
        Order GetOrderDetails();
        void PlaceOrder();
    }
}
