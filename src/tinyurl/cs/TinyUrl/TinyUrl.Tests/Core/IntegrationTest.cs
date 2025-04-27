namespace TinyUrl.Tests.Core;

public class IntegrationTest : IAsyncLifetime
{
    public TinyUrlWebFactory Factory { get; set; }
    public HttpClient HttpClient { get; set; }

    public Task InitializeAsync()
    {
        Factory = new TinyUrlWebFactory();
        HttpClient = Factory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}