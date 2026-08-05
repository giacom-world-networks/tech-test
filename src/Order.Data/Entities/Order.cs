namespace Order.Data.Entities
{
    public partial class Order
    {
        public Order()
        {
            Items = new HashSet<OrderItem>();
        }

        // Populated by EF when materialising, and explicitly on insert.
        public byte[] Id { get; set; } = null!;
        public byte[] ResellerId { get; set; } = null!;
        public byte[] CustomerId { get; set; } = null!;
        public byte[] StatusId { get; set; } = null!;
        public DateTime CreatedDate { get; set; }

        public virtual OrderStatus Status { get; set; } = null!;
        public virtual ICollection<OrderItem> Items { get; set; }
    }
}
