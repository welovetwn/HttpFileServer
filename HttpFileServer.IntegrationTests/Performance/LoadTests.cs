// HttpFileServer.IntegrationTests/Performance/LoadTests.cs
using System.Diagnostics;
using Xunit;
using FluentAssertions;
using HttpFileServer.IntegrationTests.TestFixtures;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace HttpFileServer.IntegrationTests.Performance;

public class LoadTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LoadTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task 並行首頁請求_應該在合理時間內完成()
    {
        // Arrange
        var concurrentRequests = 20;
        var stopwatch = Stopwatch.StartNew();

        // Act
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => _client.GetAsync("/"))
            .ToArray();

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // 5秒內完成
        tasks.All(t => t.Result.StatusCode != System.Net.HttpStatusCode.InternalServerError).Should().BeTrue();
    }
}