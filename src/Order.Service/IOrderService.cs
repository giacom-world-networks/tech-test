using Order.Model;

namespace Order.Service
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync(CancellationToken cancellationToken = default);

        Task<OrderDetail?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

        Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string statusName, CancellationToken cancellationToken = default);

        Task<OperationResult<OrderDetail>> UpdateOrderStatusAsync(Guid orderId, string statusName, CancellationToken cancellationToken = default);

        Task<OperationResult<OrderDetail>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

        Task<IEnumerable<MonthlyProfit>> GetCompletedOrderProfitByMonthAsync(CancellationToken cancellationToken = default);
    }
}
