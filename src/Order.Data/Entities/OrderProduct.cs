namespace Order.Data.Entities
{
    public partial class OrderProduct
    {
        public OrderProduct()
        {
            OrderItem = new HashSet<OrderItem>();
        }

        public byte[] Id { get; set; } = null!;
        public byte[] ServiceId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }

        public virtual OrderService Service { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItem { get; set; }
    }
}
