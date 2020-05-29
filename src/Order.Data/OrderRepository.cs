using Microsoft.EntityFrameworkCore;
using Order.Data.Entities;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orderEntities = await _orderContext.Order
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var orders = orderEntities.Select(x => new OrderSummary
            {
                Id = new Guid(x.Id),
                ResellerId = new Guid(x.ResellerId),
                CustomerId = new Guid(x.CustomerId),
                StatusId = new Guid(x.StatusId),
                StatusName = x.Status.Name,
                ItemCount = x.Items.Count,
                TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                CreatedDate = x.CreatedDate
            });

            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();
            
            var order = await _orderContext.Order
                .Include(x => x.Status)
                .Include(x => x.Items)
                .ThenInclude(x => x.Service)
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == orderIdBytes);

            if (order == null)
            {
                return null;
            }

            var orderDetail = new OrderDetail
            {
                Id = new Guid(order.Id),
                ResellerId = new Guid(order.ResellerId),
                CustomerId = new Guid(order.CustomerId),
                StatusId = new Guid(order.StatusId),
                StatusName = order.Status.Name,
                CreatedDate = order.CreatedDate,
                TotalCost = order.Items.Sum(x => x.Quantity * x.Product.UnitCost).Value,
                TotalPrice = order.Items.Sum(x => x.Quantity * x.Product.UnitPrice).Value,
                Items = order.Items.Select(x => new Order.Model.OrderItem
                {
                    Id = new Guid(x.Id),
                    OrderId = new Guid(x.OrderId),
                    ServiceId = new Guid(x.ServiceId),
                    ServiceName = x.Service.Name,
                    ProductId = new Guid(x.ProductId),
                    ProductName = x.Product.Name,
                    UnitCost = x.Product.UnitCost,
                    UnitPrice = x.Product.UnitPrice,
                    TotalCost = x.Product.UnitCost * x.Quantity.Value,
                    TotalPrice = x.Product.UnitPrice * x.Quantity.Value,
                    Quantity = x.Quantity.Value
                })
            };

            return orderDetail;
        }
    }
}
