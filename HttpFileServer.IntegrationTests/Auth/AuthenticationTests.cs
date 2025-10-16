// HttpFileServer.IntegrationTests/Auth/AuthenticationTests.cs
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using HttpFileServer.IntegrationTests.TestFixtures;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace HttpFileServer.IntegrationTests.Auth;

public class AuthenticationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthenticationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Application_應該可以正常啟動()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        // 只要不是 500 錯誤就算成功啟動
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    // 注意: 認證測試需要實際的 AccountController
    // 請提供您的認證 Controller 後再完善這些測試
}