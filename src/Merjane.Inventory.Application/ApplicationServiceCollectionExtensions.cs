using Merjane.Inventory.Application.Orders.ProcessOrder;
using Merjane.Inventory.Application.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Merjane.Inventory.Application;

public static class ApplicationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddSingleton(TimeProvider.System);
            services.AddScoped<ProductEventNotifier>();
            services.AddScoped<ProcessOrderHandler>();
            return services;
        }
    }
}
