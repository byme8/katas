using System.Net.Http.Json;
using TinyUrl.Models;
using TinyUrl.Tests.Core;

namespace TinyUrl.Tests;

public class TinyUrlTests : IntegrationTest
{
    [Fact]
    public async Task MissingUrl()
    {
        var response = await HttpClient.GetAsync("/missingurl");
        
        await Verify(response);
    }

    [Fact]
    public async Task AddUrl()
    {
        var request = new
        {
            url = "http://example.com/supppppppper-long-url"
        };
        var response = await HttpClient.PostAsJsonAsync("/api/urls", request);
        var serialized = await response.Content.ReadAsStringAsync();

        await Verify(new
        {
            response,
            body = serialized
        });
    }

    [Fact]
    public async Task CanGetUrl()
    {
        var request = new
        {
            url = "http://example.com/supppppppper-long-url"
        };

        var response = await HttpClient.PostAsJsonAsync("/api/urls", request);
        var redirect = await response.Content.ReadFromJsonAsync<CreateUrlResponse>();

        var redirectResponse = HttpClient.GetAsync(redirect!.Url);
        
        await Verify(redirectResponse);
    }
}