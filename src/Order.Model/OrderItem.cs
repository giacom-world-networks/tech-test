namespace Order.Model
{
    public class OrderItem
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        
        public Guid ServiceId { get; set; }
        
        public string ServiceName { get; set; } = null!;

        public Guid ProductId { get; set; }

        public string ProductName { get; set; } = null!;
        
        public int Quantity { get; set; }

        public decimal UnitCost { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalCost { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
