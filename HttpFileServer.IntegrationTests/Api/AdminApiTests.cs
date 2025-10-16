// HttpFileServer.IntegrationTests/Api/AdminApiTests.cs
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using HttpFileServer.Models;
using HttpFileServer.IntegrationTests.TestFixtures;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace HttpFileServer.IntegrationTests.Api;

public class AdminApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AdminApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetUsers_未登入_應該重導向到登入頁()
    {
        // Act
        var response = await _client.GetAsync("/admin/users");

        // Assert
        // 根據您的認證設定，可能是 Unauthorized 或 Redirect
        (response.StatusCode == HttpStatusCode.Unauthorized || 
         response.StatusCode == HttpStatusCode.Redirect).Should().BeTrue();
    }

    // 注意: 以下測試需要實際的 Controller 和認證機制才能正確運作
    // 暫時註解掉，等您提供 Controller 程式碼後再啟用

    /*
    [Fact]
    public async Task Login_使用正確帳密_應該成功()
    {
        // Arrange
        var loginData = new { username = "testadmin", password = "testpass" };

        // Act
        var response = await _client.PostAsJsonAsync("/account/login", loginData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    */
}