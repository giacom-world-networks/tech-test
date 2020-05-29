using System;
using System.Collections.Generic;

namespace Order.Data.Entities
{
    public partial class Order
    {
        public Order()
        {
            Items = new HashSet<OrderItem>();
        }

        public byte[] Id { get; set; }
        public byte[] ResellerId { get; set; }
        public byte[] CustomerId { get; set; }
        public byte[] StatusId { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual OrderStatus Status { get; set; }
        public virtual ICollection<OrderItem> Items { get; set; }
    }
}
