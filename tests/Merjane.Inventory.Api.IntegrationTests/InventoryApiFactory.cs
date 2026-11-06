using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Refacto.DotNet.Controllers.Services;

namespace Merjane.Inventory.Api.IntegrationTests;

/// <summary>Runs the real API on an isolated database, with a fixed clock and a mocked notification service.</summary>
public sealed class InventoryApiFactory : WebApplicationFactory<Program>
{
    public static readonly DateTime Now = new(2026, 6, 15, 10, 0, 0);

    public Mock<INotificationService> NotificationService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:InventoryDatabase", Guid.NewGuid().ToString());
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(Now, TimeSpan.Zero)));
            services.AddSingleton(NotificationService.Object);
        });
    }
}
