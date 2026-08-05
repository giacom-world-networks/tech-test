using Order.Data;
using Order.Model;

namespace Order.Service
{
    public class OrderService : IOrderService
    {
        /// <summary>
        /// Profit is only reported for orders that reached this status.
        /// </summary>
        private const string CompletedStatus = "Completed";

        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync(CancellationToken cancellationToken = default)
        {
            var orders = await _orderRepository.GetOrdersAsync(cancellationToken);
            return orders;
        }

        public async Task<OrderDetail?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId, cancellationToken);
            return order;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersByStatusAsync(string statusName, CancellationToken cancellationToken = default)
        {
            var orders = await _orderRepository.GetOrdersByStatusAsync(statusName, cancellationToken);
            return orders;
        }

        public async Task<OperationResult<OrderDetail>> UpdateOrderStatusAsync(Guid orderId, string statusName, CancellationToken cancellationToken = default)
        {
            var result = await _orderRepository.UpdateOrderStatusAsync(orderId, statusName, cancellationToken);
            return result;
        }

        public async Task<OperationResult<OrderDetail>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                return OperationResult<OrderDetail>.Invalid("An order must be supplied.");
            }

            var result = await _orderRepository.CreateOrderAsync(request, cancellationToken);
            return result;
        }

        public async Task<IEnumerable<MonthlyProfit>> GetCompletedOrderProfitByMonthAsync(CancellationToken cancellationToken = default)
        {
            var profit = await _orderRepository.GetProfitByMonthAsync(CompletedStatus, cancellationToken);
            return profit;
        }
    }
}
