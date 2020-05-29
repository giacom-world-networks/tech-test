using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.Data
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();

        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);
    }
}
