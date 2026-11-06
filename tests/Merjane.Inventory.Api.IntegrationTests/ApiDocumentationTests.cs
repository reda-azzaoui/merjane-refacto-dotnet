using System.Net;

namespace Merjane.Inventory.Api.IntegrationTests;

[Collection("Sequential")]
public sealed class ApiDocumentationTests : IDisposable
{
    private readonly InventoryApiFactory _factory = new();
    private readonly HttpClient _client;

    public ApiDocumentationTests() => _client = _factory.CreateClient();

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Swagger_ui_is_served_in_development()
    {
        HttpResponseMessage response = await _client.GetAsync(new Uri("/swagger/index.html", UriKind.Relative), CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task OpenApi_description_documents_the_process_order_endpoint()
    {
        string description = await _client.GetStringAsync(new Uri("/openapi/v1.json", UriKind.Relative), CancellationToken);

        Assert.Contains("/orders/{orderId}/processOrder", description, StringComparison.Ordinal);
    }
}
