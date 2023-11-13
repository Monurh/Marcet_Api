using System.Runtime.Serialization;

namespace Marcet_DB.Models
{
    public class Product
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal? Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Photo { get; set; } = "";
    }

    public enum SortProduct
    {
        [EnumMember(Value = "А-Я")]
        NameAsc, // по Алфовиту
        [EnumMember(Value = "Я-А")]
        NameDesc, // не по алфовиту
        [EnumMember(Value = "За зростанням ")]
        PriceAsc, // повазростанию цены
        [EnumMember(Value = "За спаданням")]
        PriceDesc, // по убыванию цены
        [EnumMember(Value = "За категоріями")]
        Category
    }
}
