using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerLedger.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CareerLedger.UnitTests.Domain.Entities;

public class AccountTests
{
    [Fact]
    public void Create_WithValidData_ReturnsAccount()
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = "hashed_password";

        // Act
        var account = Account.Create(email, passwordHash);

        // Assert
        account.Should().NotBeNull();
        account.Id.Should().NotBe(Guid.Empty);
        account.Email.Should().Be("test@example.com");
        account.PasswordHash.Should().Be(passwordHash);
        account.IsActive.Should().BeTrue();
        account.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithInvalidEmail_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => Account.Create("not-an-email", "hash"));
    }
}
