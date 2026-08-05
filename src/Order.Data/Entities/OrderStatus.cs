namespace Order.Data.Entities
{
    public partial class OrderStatus
    {
        public OrderStatus()
        {
            Order = new HashSet<Order>();
        }

        public byte[] Id { get; set; } = null!;
        public string Name { get; set; } = null!;

        public virtual ICollection<Order> Order { get; set; }
    }
}
