using Xunit;
using FluentAssertions;
using CareerLedger.Domain.Enums;
using CareerLedger.Domain.Entities;

namespace CareerLedger.UnitTests.Domain.Entities
{
    public class ApplicationTests
    {
        [Fact]
        public void Create_WithValidData_ReturnsApplicationWithSubmittedEvent()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var companyName = "TechCorp";
            var jobTitle = "Senior Engineer";

            // Act
            var application = CareerLedger.Domain.Entities.Application.Create(accountId, companyName, jobTitle);

            // Assert
            application.Should().NotBeNull();
            application.CompanyName.Should().Be(companyName);
            application.JobTitle.Should().Be(jobTitle);
            application.Events.Should().HaveCount(1);
            application.Events.First().EventType.Should().Be(EventType.Submitted);
            application.CurrentStatus.Should().Be(EventType.Submitted);
        }

        [Fact]
        public void CurrentStatus_WithMultipleEvents_ReturnsLatestEvent()
        {
            // Arrange
            var application = CareerLedger.Domain.Entities.Application.Create(Guid.NewGuid(), "Company", "Title");
            var reviewEvent = ApplicationEvent.Create(
                application.Id,
                application.AccountId,
                EventType.InReview,
                DateTime.UtcNow
            );
            application.AddEvent(reviewEvent);

            // Act
            var status = application.CurrentStatus;

            // Assert
            status.Should().Be(EventType.InReview);
        }
    }
}
