using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Data;
using Order.Data.Entities;
using Order.Service;

namespace OrderService.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((builder, services) =>
                {
                    services.AddDbContext<OrderContext>(options =>
                    {
                        var serviceOptions = builder.Configuration["OrderConnectionString"];
                        options
                        .UseLazyLoadingProxies()
                        .UseMySQL(serviceOptions);
                    });

                    services.AddScoped<IOrderService, Order.Service.OrderService>();
                    services.AddScoped<IOrderRepository, OrderRepository>();
                });
    }
}
