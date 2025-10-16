// HttpFileServer.Tests/Services/PermissionServiceTests.cs
using Xunit;
using FluentAssertions;
using HttpFileServer.Models;

namespace HttpFileServer.Tests.Services;

public class PermissionLevelTests
{
    [Theory]
    [InlineData(PermissionLevel.Admin, PermissionLevel.ReadOnly, true)]
    [InlineData(PermissionLevel.FullAccess, PermissionLevel.ReadOnly, true)]
    [InlineData(PermissionLevel.ReadOnly, PermissionLevel.FullAccess, false)]
    [InlineData(PermissionLevel.ReadOnly, PermissionLevel.ReadOnly, true)]
    [InlineData(PermissionLevel.None, PermissionLevel.ReadOnly, false)]
    public void PermissionLevel_比較測試(PermissionLevel userPerm, PermissionLevel requiredPerm, bool expected)
    {
        // Act
        var result = (int)userPerm >= (int)requiredPerm;

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void PermissionLevel_Admin應該是最高權限()
    {
        // Assert
        ((int)PermissionLevel.Admin).Should().BeGreaterThan((int)PermissionLevel.FullAccess);
        ((int)PermissionLevel.Admin).Should().BeGreaterThan((int)PermissionLevel.ReadOnly);
        ((int)PermissionLevel.Admin).Should().BeGreaterThan((int)PermissionLevel.None);
    }

    [Fact]
    public void PermissionLevel_FullAccess應該大於ReadOnly()
    {
        // Assert
        ((int)PermissionLevel.FullAccess).Should().BeGreaterThan((int)PermissionLevel.ReadOnly);
    }

    [Fact]
    public void PermissionLevel_None應該是最低權限()
    {
        // Assert
        ((int)PermissionLevel.None).Should().BeLessThan((int)PermissionLevel.ReadOnly);
        ((int)PermissionLevel.None).Should().BeLessThan((int)PermissionLevel.FullAccess);
        ((int)PermissionLevel.None).Should().BeLessThan((int)PermissionLevel.Admin);
    }
}