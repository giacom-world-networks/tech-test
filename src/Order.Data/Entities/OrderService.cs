namespace Order.Data.Entities
{
    public partial class OrderService
    {
        public OrderService()
        {
            OrderItem = new HashSet<OrderItem>();
            OrderProduct = new HashSet<OrderProduct>();
        }

        public byte[] Id { get; set; } = null!;
        public string Name { get; set; } = null!;

        public virtual ICollection<OrderItem> OrderItem { get; set; }
        public virtual ICollection<OrderProduct> OrderProduct { get; set; }
    }
}
