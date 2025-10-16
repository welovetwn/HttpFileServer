// HttpFileServer.IntegrationTests/Scenarios/UserWorkflowTests.cs
using System.Net;
using Xunit;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using HttpFileServer.IntegrationTests.TestFixtures;

namespace HttpFileServer.IntegrationTests.Scenarios;

public class UserWorkflowTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UserWorkflowTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task 首頁應該可以存取()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    // 完整的工作流程測試需要：
    // 1. AccountController (登入/登出)
    // 2. DashboardController (儀表板)
    // 3. FileController (檔案操作)
    // 請提供這些 Controller 的程式碼後，我會補充完整的測試
}