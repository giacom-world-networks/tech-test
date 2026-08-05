using Order.Model;

namespace Order.Data
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync(CancellationToken cancellationToken = default);

        Task<OrderDetail?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

        Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string statusName, CancellationToken cancellationToken = default);

        Task<OperationResult<OrderDetail>> UpdateOrderStatusAsync(Guid orderId, string statusName, CancellationToken cancellationToken = default);

        Task<OperationResult<OrderDetail>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

        Task<IEnumerable<MonthlyProfit>> GetProfitByMonthAsync(string statusName, CancellationToken cancellationToken = default);
    }
}
