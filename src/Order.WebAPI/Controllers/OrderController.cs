using Microsoft.AspNetCore.Mvc;
using Order.Model;
using Order.Service;

namespace OrderService.WebAPI.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderSummary>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var orders = await _orderService.GetOrdersAsync(cancellationToken);
            return Ok(orders);
        }

        /// <summary>
        /// Sits above the "{orderId}" route because literal segments take
        /// precedence over route parameters, so /orders/profit is unambiguous.
        /// </summary>
        [HttpGet("profit")]
        [ProducesResponseType(typeof(IEnumerable<MonthlyProfit>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfitByMonth(CancellationToken cancellationToken)
        {
            var profit = await _orderService.GetCompletedOrderProfitByMonthAsync(cancellationToken);
            return Ok(profit);
        }

        [HttpGet("status/{statusName}")]
        [ProducesResponseType(typeof(IEnumerable<OrderSummary>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersByStatus(string statusName, CancellationToken cancellationToken)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(statusName, cancellationToken);
            return Ok(orders);
        }

        [HttpGet("{orderId:guid}")]
        [ProducesResponseType(typeof(OrderDetail), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid orderId, CancellationToken cancellationToken)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId, cancellationToken);
            if (order != null)
            {
                return Ok(order);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(OrderDetail), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
        {
            var result = await _orderService.CreateOrderAsync(request, cancellationToken);
            if (!result.IsSuccess)
            {
                return MapFailure(result);
            }

            return CreatedAtAction(nameof(GetOrderById), new { orderId = result.Value!.Id }, result.Value);
        }

        [HttpPut("{orderId:guid}/status")]
        [ProducesResponseType(typeof(OrderDetail), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
        {
            // [ApiController] short-circuits on invalid ModelState, so [Required]
            // guarantees Status is present by the time this runs.
            var result = await _orderService.UpdateOrderStatusAsync(orderId, request.Status!, cancellationToken);
            if (!result.IsSuccess)
            {
                return MapFailure(result);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Translates a failed service outcome into the matching HTTP response.
        /// </summary>
        private IActionResult MapFailure(OperationResult<OrderDetail> result)
        {
            if (result.Outcome == OperationOutcome.NotFound)
            {
                return NotFound();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return ValidationProblem(ModelState);
        }
    }
}
