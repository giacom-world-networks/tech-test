namespace Order.Data.Entities
{
    public partial class OrderItem
    {
        public byte[] Id { get; set; } = null!;
        public byte[] OrderId { get; set; } = null!;
        public byte[] ProductId { get; set; } = null!;
        public byte[] ServiceId { get; set; } = null!;

        /// <summary>Nullable in the schema, so callers must tolerate a missing quantity.</summary>
        public int? Quantity { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual OrderProduct Product { get; set; } = null!;
        public virtual OrderService Service { get; set; } = null!;
    }
}
