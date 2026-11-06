using Merjane.Inventory.Application.Common;
using Merjane.Inventory.Domain.Orders;
using Merjane.Inventory.Domain.Products;
using Merjane.Inventory.Infrastructure.Persistence;
using Merjane.Inventory.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refacto.DotNet.Controllers.Services;
using Refacto.DotNet.Controllers.Services.Impl;

namespace Merjane.Inventory.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    private const string InventoryDatabaseConnectionName = "InventoryDatabase";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            string databaseName = configuration.GetConnectionString(InventoryDatabaseConnectionName)
                ?? throw new InvalidOperationException($"Connection string '{InventoryDatabaseConnectionName}' is missing.");

            services.AddDbContext<InventoryDbContext>(options => options.UseInMemoryDatabase(databaseName));
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUnitOfWork, EntityFrameworkUnitOfWork>();
            services.AddScoped<INotificationService, NotificationService>();
            return services;
        }
    }
}
