using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;
using Order.Data;
using Order.Data.Entities;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Service.Tests
{
    public class OrderServiceTests
    {
        // Assigned in [SetUp], which NUnit runs before every test.
        private IOrderService _orderService = null!;
        private IOrderRepository _orderRepository = null!;
        private OrderContext _orderContext = null!;
        private DbConnection _connection = null!;

        private readonly byte[] _orderStatusCreatedId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderStatusInProgressId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderStatusFailedId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderStatusCompletedId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderServiceEmailId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderProductEmailId = Guid.NewGuid().ToByteArray();


        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<OrderContext>()
                .UseSqlite(CreateInMemoryDatabase())
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .Options;

            _connection = RelationalOptionsExtension.Extract(options).Connection!;

            _orderContext = new OrderContext(options);
            _orderContext.Database.EnsureDeleted();
            _orderContext.Database.EnsureCreated();

            _orderRepository = new OrderRepository(_orderContext, TimeProvider.System);
            _orderService = new OrderService(_orderRepository);

            await AddReferenceDataAsync(_orderContext);
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Dispose();
            _orderContext.Dispose();
        }


        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            return connection;
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsCorrectNumberOfOrders()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            Assert.That(orders.Count(), Is.EqualTo(3));
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsOrdersWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            var order1 = orders.Single(x => x.Id == orderId1);
            var order2 = orders.Single(x => x.Id == orderId2);
            var order3 = orders.Single(x => x.Id == orderId3);

            Assert.That(order1.TotalCost, Is.EqualTo(0.8m));
            Assert.That(order1.TotalPrice, Is.EqualTo(0.9m));

            Assert.That(order2.TotalCost, Is.EqualTo(1.6m));
            Assert.That(order2.TotalPrice, Is.EqualTo(1.8m));

            Assert.That(order3.TotalCost, Is.EqualTo(2.4m));
            Assert.That(order3.TotalPrice, Is.EqualTo(2.7m));
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrder()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.That(order!.Id, Is.EqualTo(orderId1));
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrderItemCount()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.That(order!.Items.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsOrderWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 2);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.That(order!.TotalCost, Is.EqualTo(1.6m));
            Assert.That(order.TotalPrice, Is.EqualTo(1.8m));
        }

        #region Null quantity handling

        [Test]
        public async Task GetOrderByIdAsync_TreatsNullQuantityAsZero()
        {
            // Arrange - the schema allows order_item.Quantity to be NULL.
            var orderId = Guid.NewGuid();
            await AddOrder(orderId, 2);
            await AddOrderItemWithNullQuantity(orderId);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            Assert.That(order, Is.Not.Null);
            Assert.That(order!.Items.Count(), Is.EqualTo(2));

            var nullQuantityItem = order.Items.Single(x => x.Quantity == 0);
            Assert.That(nullQuantityItem.TotalCost, Is.EqualTo(0m));
            Assert.That(nullQuantityItem.TotalPrice, Is.EqualTo(0m));

            // The priced item still contributes its full total.
            Assert.That(order.TotalCost, Is.EqualTo(1.6m));
            Assert.That(order.TotalPrice, Is.EqualTo(1.8m));
        }

        [Test]
        public async Task GetOrdersAsync_ToleratesNullQuantity()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            await AddOrder(orderId, 1);
            await AddOrderItemWithNullQuantity(orderId);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            Assert.That(orders.Single().TotalCost, Is.EqualTo(0.8m));
            Assert.That(orders.Single().ItemCount, Is.EqualTo(2));
        }

        [Test]
        public async Task GetCompletedOrderProfitByMonthAsync_ToleratesNullQuantity()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            await AddOrder(orderId, 1, _orderStatusCompletedId, new DateTime(2024, 5, 1));
            await AddOrderItemWithNullQuantity(orderId);

            // Act
            var profit = (await _orderService.GetCompletedOrderProfitByMonthAsync()).ToList();

            // Assert
            Assert.That(profit.Single().Profit, Is.EqualTo(0.1m));
        }

        #endregion

        #region Task 1 - orders by status

        [Test]
        public async Task GetOrdersByStatusAsync_ReturnsOnlyOrdersWithThatStatus()
        {
            // Arrange
            var createdOrderId = Guid.NewGuid();
            await AddOrder(createdOrderId, 1, _orderStatusCreatedId, DateTime.Now);

            var failedOrderId = Guid.NewGuid();
            await AddOrder(failedOrderId, 1, _orderStatusFailedId, DateTime.Now);

            // Act
            var failedOrders = await _orderService.GetOrdersByStatusAsync("Failed");

            // Assert
            Assert.That(failedOrders.Count(), Is.EqualTo(1));
            Assert.That(failedOrders.Single().Id, Is.EqualTo(failedOrderId));
            Assert.That(failedOrders.Single().StatusName, Is.EqualTo("Failed"));
        }

        [Test]
        public async Task GetOrdersByStatusAsync_MatchesStatusNameCaseInsensitively()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            await AddOrder(orderId, 1, _orderStatusInProgressId, DateTime.Now);

            // Act
            var orders = await _orderService.GetOrdersByStatusAsync("in progress");

            // Assert
            Assert.That(orders.Count(), Is.EqualTo(1));
            Assert.That(orders.Single().Id, Is.EqualTo(orderId));
        }

        [Test]
        public async Task GetOrdersByStatusAsync_ReturnsEmptyForUnknownStatus()
        {
            // Arrange
            await AddOrder(Guid.NewGuid(), 1);

            // Act
            var orders = await _orderService.GetOrdersByStatusAsync("Nonexistent");

            // Assert
            Assert.That(orders, Is.Empty);
        }

        #endregion

        #region Task 2 - update order status

        [Test]
        public async Task UpdateOrderStatusAsync_ChangesTheStatus()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            await AddOrder(orderId, 1);

            // Act
            var result = await _orderService.UpdateOrderStatusAsync(orderId, "In Progress");

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.StatusName, Is.EqualTo("In Progress"));

            var reloaded = await _orderService.GetOrderByIdAsync(orderId);
            Assert.That(reloaded!.StatusName, Is.EqualTo("In Progress"));
        }

        [Test]
        public async Task UpdateOrderStatusAsync_ReturnsNotFoundForUnknownOrder()
        {
            // Act
            var result = await _orderService.UpdateOrderStatusAsync(Guid.NewGuid(), "Completed");

            // Assert
            Assert.That(result.Outcome, Is.EqualTo(OperationOutcome.NotFound));
        }

        [Test]
        public async Task UpdateOrderStatusAsync_ReturnsInvalidForUnknownStatus()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            await AddOrder(orderId, 1);

            // Act
            var result = await _orderService.UpdateOrderStatusAsync(orderId, "Teleported");

            // Assert
            Assert.That(result.Outcome, Is.EqualTo(OperationOutcome.Invalid));
            Assert.That(result.Errors, Is.Not.Empty);

            var unchanged = await _orderService.GetOrderByIdAsync(orderId);
            Assert.That(unchanged!.StatusName, Is.EqualTo("Created"));
        }

        #endregion

        #region Task 3 - create order

        [Test]
        public async Task CreateOrderAsync_CreatesOrderWithCorrectTotals()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItemRequest>
                {
                    new CreateOrderItemRequest { ProductId = new Guid(_orderProductEmailId), Quantity = 2 }
                }
            };

            // Act
            var result = await _orderService.CreateOrderAsync(request);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.ResellerId, Is.EqualTo(request.ResellerId));
            Assert.That(result.Value.CustomerId, Is.EqualTo(request.CustomerId));
            Assert.That(result.Value.StatusName, Is.EqualTo("Created"));
            Assert.That(result.Value.Items.Count(), Is.EqualTo(1));
            Assert.That(result.Value.TotalCost, Is.EqualTo(1.6m));
            Assert.That(result.Value.TotalPrice, Is.EqualTo(1.8m));
        }

        [Test]
        public async Task CreateOrderAsync_DerivesServiceIdFromTheProduct()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItemRequest>
                {
                    new CreateOrderItemRequest { ProductId = new Guid(_orderProductEmailId), Quantity = 1 }
                }
            };

            // Act
            var result = await _orderService.CreateOrderAsync(request);

            // Assert
            Assert.That(result.Value!.Items.Single().ServiceId, Is.EqualTo(new Guid(_orderServiceEmailId)));
            Assert.That(result.Value.Items.Single().ServiceName, Is.EqualTo("Email"));
        }

        [Test]
        public async Task CreateOrderAsync_IsRetrievableAfterwards()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItemRequest>
                {
                    new CreateOrderItemRequest { ProductId = new Guid(_orderProductEmailId), Quantity = 3 }
                }
            };

            // Act
            var created = await _orderService.CreateOrderAsync(request);
            var fetched = await _orderService.GetOrderByIdAsync(created.Value!.Id);

            // Assert
            Assert.That(fetched, Is.Not.Null);
            Assert.That(fetched!.Id, Is.EqualTo(created.Value.Id));
            Assert.That(fetched.TotalCost, Is.EqualTo(2.4m));
        }

        [Test]
        public async Task CreateOrderAsync_ReturnsInvalidForUnknownProduct()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Items = new List<CreateOrderItemRequest>
                {
                    new CreateOrderItemRequest { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            // Act
            var result = await _orderService.CreateOrderAsync(request);

            // Assert
            Assert.That(result.Outcome, Is.EqualTo(OperationOutcome.Invalid));
            Assert.That(result.Errors, Is.Not.Empty);

            var orders = await _orderService.GetOrdersAsync();
            Assert.That(orders, Is.Empty);
        }

        [Test]
        public async Task CreateOrderAsync_ReturnsInvalidForNullRequest()
        {
            // Act
            var result = await _orderService.CreateOrderAsync(null!);

            // Assert
            Assert.That(result.Outcome, Is.EqualTo(OperationOutcome.Invalid));
        }

        #endregion

        #region Task 4 - profit by month

        [Test]
        public async Task GetCompletedOrderProfitByMonthAsync_GroupsByMonthAndYear()
        {
            // Arrange - unit cost 0.8, unit price 0.9, so profit is 0.1 per unit.
            await AddOrder(Guid.NewGuid(), 1, _orderStatusCompletedId, new DateTime(2024, 1, 10));
            await AddOrder(Guid.NewGuid(), 2, _orderStatusCompletedId, new DateTime(2024, 1, 20));
            await AddOrder(Guid.NewGuid(), 4, _orderStatusCompletedId, new DateTime(2024, 2, 5));

            // Act
            var profit = (await _orderService.GetCompletedOrderProfitByMonthAsync()).ToList();

            // Assert
            Assert.That(profit.Count, Is.EqualTo(2));

            var january = profit.Single(x => x.Year == 2024 && x.Month == 1);
            Assert.That(january.TotalCost, Is.EqualTo(2.4m));
            Assert.That(january.TotalPrice, Is.EqualTo(2.7m));
            Assert.That(january.Profit, Is.EqualTo(0.3m));

            var february = profit.Single(x => x.Year == 2024 && x.Month == 2);
            Assert.That(february.Profit, Is.EqualTo(0.4m));
        }

        [Test]
        public async Task GetCompletedOrderProfitByMonthAsync_IgnoresOrdersThatAreNotCompleted()
        {
            // Arrange
            await AddOrder(Guid.NewGuid(), 1, _orderStatusCompletedId, new DateTime(2024, 3, 1));
            await AddOrder(Guid.NewGuid(), 10, _orderStatusFailedId, new DateTime(2024, 3, 2));
            await AddOrder(Guid.NewGuid(), 10, _orderStatusCreatedId, new DateTime(2024, 3, 3));

            // Act
            var profit = (await _orderService.GetCompletedOrderProfitByMonthAsync()).ToList();

            // Assert
            Assert.That(profit.Count, Is.EqualTo(1));
            Assert.That(profit.Single().Profit, Is.EqualTo(0.1m));
        }

        [Test]
        public async Task GetCompletedOrderProfitByMonthAsync_ReturnsEmptyWhenNoCompletedOrders()
        {
            // Arrange
            await AddOrder(Guid.NewGuid(), 1, _orderStatusFailedId, new DateTime(2024, 4, 1));

            // Act
            var profit = await _orderService.GetCompletedOrderProfitByMonthAsync();

            // Assert
            Assert.That(profit, Is.Empty);
        }

        #endregion

        private async Task AddOrder(Guid orderId, int quantity)
        {
            await AddOrder(orderId, quantity, _orderStatusCreatedId, DateTime.Now);
        }

        private async Task AddOrderItemWithNullQuantity(Guid orderId)
        {
            _orderContext.OrderItem.Add(new Data.Entities.OrderItem
            {
                Id = Guid.NewGuid().ToByteArray(),
                OrderId = orderId.ToByteArray(),
                ServiceId = _orderServiceEmailId,
                ProductId = _orderProductEmailId,
                Quantity = null
            });

            await _orderContext.SaveChangesAsync();
        }

        private async Task AddOrder(Guid orderId, int quantity, byte[] statusId, DateTime createdDate)
        {
            var orderIdBytes = orderId.ToByteArray();
            _orderContext.Order.Add(new Data.Entities.Order
            {
                Id = orderIdBytes,
                ResellerId = Guid.NewGuid().ToByteArray(),
                CustomerId = Guid.NewGuid().ToByteArray(),
                CreatedDate = createdDate,
                StatusId = statusId,
            });

            _orderContext.OrderItem.Add(new Data.Entities.OrderItem
            {
                Id = Guid.NewGuid().ToByteArray(),
                OrderId = orderIdBytes,
                ServiceId = _orderServiceEmailId,
                ProductId = _orderProductEmailId,
                Quantity = quantity
            });

            await _orderContext.SaveChangesAsync();
        }

        private async Task AddReferenceDataAsync(OrderContext orderContext)
        {
            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusCreatedId,
                Name = "Created",
            });

            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusInProgressId,
                Name = "In Progress",
            });

            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusFailedId,
                Name = "Failed",
            });

            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusCompletedId,
                Name = "Completed",
            });

            orderContext.OrderService.Add(new Data.Entities.OrderService
            {
                Id = _orderServiceEmailId,
                Name = "Email"
            });

            orderContext.OrderProduct.Add(new OrderProduct
            {
                Id = _orderProductEmailId,
                Name = "100GB Mailbox",
                UnitCost = 0.8m,
                UnitPrice = 0.9m,
                ServiceId = _orderServiceEmailId
            });

            await orderContext.SaveChangesAsync();
        }
    }
}
