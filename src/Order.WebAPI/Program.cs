using Microsoft.EntityFrameworkCore;
using Order.Data;
using Order.Service;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["OrderConnectionString"]
    ?? throw new InvalidOperationException("Configuration value 'OrderConnectionString' is missing.");

builder.Services.AddDbContext<OrderContext>(options =>
{
    options
        .UseLazyLoadingProxies()
        .UseMySQL(connectionString);
});

builder.Services.AddSingleton(TimeProvider.System);
// Fully qualified: "OrderService" alone binds to the OrderService.WebAPI namespace.
builder.Services.AddScoped<IOrderService, Order.Service.OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddControllers();

// Turns unhandled exceptions and bare status codes into RFC 9457 responses,
// matching the shape validation failures already return.
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();

// The container serves plain HTTP on 8080, so redirecting is only meaningful
// when an HTTPS port has actually been configured.
if (app.Environment.IsDevelopment() || app.Configuration["ASPNETCORE_HTTPS_PORTS"] != null)
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapOpenApi();

app.Run();
