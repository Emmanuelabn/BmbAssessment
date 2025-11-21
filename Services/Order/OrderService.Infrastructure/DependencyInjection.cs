using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.External;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // SQL Server for Orders
            var connectionString = configuration.GetConnectionString("OrderDatabase");

            services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<IOrderRepository, OrderRepository>();

            // Typed HttpClient for ProductService
            var productBaseUrl = configuration["ProductService:BaseUrl"];
            if (string.IsNullOrWhiteSpace(productBaseUrl))
            {
                throw new InvalidOperationException("ProductService:BaseUrl must be configured in appsettings.");
            }

            services.AddHttpClient<IProductReadService, ProductReadServiceHttpClient>(client =>
            {
                client.BaseAddress = new Uri(productBaseUrl);
            });

            return services;
        }
    }
}
