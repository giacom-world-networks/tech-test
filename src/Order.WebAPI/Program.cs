using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order.Service;
using OrderService.Data;
using OrderService.Data.Entities;

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
                        options.UseMySQL(serviceOptions);
                    });

                    services.AddScoped<IOrderService, Order.Service.OrderService>();
                    services.AddScoped<IOrderRepository, OrderRepository>();
                });
    }
}
