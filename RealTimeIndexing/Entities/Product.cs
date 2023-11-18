namespace RealTimeIndexing.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string QuantityPerUnit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; } = decimal.Zero;
        public short UnitsInStock { get; set; }
    }
}
