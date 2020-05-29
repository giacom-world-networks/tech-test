using System;
using System.Collections.Generic;

namespace Order.Model
{
    public class OrderDetail
    {
        public Guid Id { get; set; }
        
        public Guid ResellerId { get; set; }
        
        public Guid CustomerId { get; set; }
        
        public Guid StatusId { get; set; }

        public string StatusName { get; set; }

        public DateTime CreatedDate { get; set; }

        public decimal TotalCost { get; set; }

        public decimal TotalPrice { get; set; }

        public IEnumerable<OrderItem> Items { get; set; }

    }
}
