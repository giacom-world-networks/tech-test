namespace OrderService.Data.Entities
{
    public partial class OrderItem
    {
        public byte[] Id { get; set; }
        public byte[] OrderId { get; set; }
        public byte[] ProductId { get; set; }
        public byte[] ServiceId { get; set; }
        public int? Quantity { get; set; }

        public virtual Order Order { get; set; }
        public virtual OrderProduct Product { get; set; }
        public virtual OrderService Service { get; set; }
    }
}
