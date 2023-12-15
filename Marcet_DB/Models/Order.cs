namespace Marcet_DB.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTime OrderDateTime { get; set; }
        public decimal OrderTotal { get; set; }
        public string OrderStatus { get; set; } = "";
        public string CustomerInformation { get; set; } = "";
        public List<Product> Products { get; set; } 
    }
}
