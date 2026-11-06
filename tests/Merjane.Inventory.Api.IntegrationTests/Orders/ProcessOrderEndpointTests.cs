using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Merjane.Inventory.Domain.Orders;
using Merjane.Inventory.Domain.Products;
using Merjane.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Merjane.Inventory.Api.IntegrationTests.Orders;

[Collection("Sequential")]
public sealed class ProcessOrderEndpointTests : IDisposable
{
    private static readonly DateTime Now = InventoryApiFactory.Now;
    private readonly InventoryApiFactory _factory = new();
    private readonly HttpClient _client;

    public ProcessOrderEndpointTests() => _client = _factory.CreateClient();

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Processing_an_order_applies_the_rules_of_each_product_family()
    {
        DateTime milkExpiryDate = Now.AddDays(-2);
        Product usbCable = new NormalProduct("USB Cable", 30, 15);
        Product usbDongle = new NormalProduct("USB Dongle", 0, 10);
        Product butter = new ExpirableProduct("Butter", 30, 15, Now.AddDays(26));
        Product milk = new ExpirableProduct("Milk", 6, 90, milkExpiryDate);
        Product watermelon = new SeasonalProduct("Watermelon", 30, 15, new SeasonPeriod(Now.AddDays(-2), Now.AddDays(58)));
        Product grapes = new SeasonalProduct("Grapes", 30, 15, new SeasonPeriod(Now.AddDays(180), Now.AddDays(240)));
        long orderId = await SeedOrderAsync(usbCable, usbDongle, butter, milk, watermelon, grapes);

        HttpResponseMessage response = await PostProcessOrderAsync(orderId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>(CancellationToken);
        Assert.Equal(orderId, body.GetProperty("id").GetInt64());

        Assert.Equal(29, await GetAvailableQuantityAsync(usbCable.Id));
        Assert.Equal(0, await GetAvailableQuantityAsync(usbDongle.Id));
        Assert.Equal(29, await GetAvailableQuantityAsync(butter.Id));
        Assert.Equal(0, await GetAvailableQuantityAsync(milk.Id));
        Assert.Equal(29, await GetAvailableQuantityAsync(watermelon.Id));
        Assert.Equal(30, await GetAvailableQuantityAsync(grapes.Id));

        _factory.NotificationService.Verify(service => service.SendDelayNotification(10, "USB Dongle"), Times.Once());
        _factory.NotificationService.Verify(service => service.SendExpirationNotification("Milk", milkExpiryDate), Times.Once());
        _factory.NotificationService.Verify(service => service.SendOutOfStockNotification("Grapes"), Times.Once());
        _factory.NotificationService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Processing_an_order_twice_updates_the_stock_twice()
    {
        Product product = new NormalProduct("USB Cable", 5, 15);
        long orderId = await SeedOrderAsync(product);

        HttpResponseMessage firstResponse = await PostProcessOrderAsync(orderId);
        HttpResponseMessage secondResponse = await PostProcessOrderAsync(orderId);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.Equal(3, await GetAvailableQuantityAsync(product.Id));
    }

    [Fact]
    public async Task Processing_an_order_without_products_succeeds_and_notifies_nobody()
    {
        long orderId = await SeedOrderAsync();

        HttpResponseMessage response = await PostProcessOrderAsync(orderId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _factory.NotificationService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Unknown_order_returns_a_not_found_problem()
    {
        HttpResponseMessage response = await PostProcessOrderAsync(999);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Non_numeric_order_id_returns_a_bad_request()
    {
        HttpResponseMessage response = await _client.PostAsync(new Uri("/orders/abc/processOrder", UriKind.Relative), null, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Customers_are_notified_only_after_the_changes_are_saved()
    {
        Product milk = new ExpirableProduct("Milk", 6, 90, Now.AddDays(-2));
        long orderId = await SeedOrderAsync(milk);
        int? quantitySavedWhenNotified = null;
        _factory.NotificationService
            .Setup(service => service.SendExpirationNotification("Milk", It.IsAny<DateTime>()))
            .Callback(() => quantitySavedWhenNotified = GetAvailableQuantityAsync(milk.Id).GetAwaiter().GetResult());

        await PostProcessOrderAsync(orderId);

        Assert.Equal(0, quantitySavedWhenNotified);
    }

    private Task<HttpResponseMessage> PostProcessOrderAsync(long orderId) =>
        _client.PostAsync(new Uri($"/orders/{orderId}/processOrder", UriKind.Relative), null, CancellationToken);

    private async Task<long> SeedOrderAsync(params Product[] products)
    {
        await using AsyncServiceScope scope = _factory.Services.CreateAsyncScope();
        InventoryDbContext dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        dbContext.Products.AddRange(products);
        await dbContext.SaveChangesAsync(CancellationToken);

        Order order = new(products.Select(product => product.Id));
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(CancellationToken);

        return order.Id;
    }

    private async Task<int> GetAvailableQuantityAsync(long productId)
    {
        await using AsyncServiceScope scope = _factory.Services.CreateAsyncScope();
        InventoryDbContext dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        Product product = await dbContext.Products.AsNoTracking().SingleAsync(candidate => candidate.Id == productId, CancellationToken);
        return product.AvailableQuantity;
    }
}
