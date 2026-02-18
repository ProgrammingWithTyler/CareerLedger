using CareerLedger.Domain.Entities;
using CareerLedger.Domain.Enums;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CareerLedger.UnitTests.Domain.Entities
{
    public class ApplicationEventTests
    {
        [Fact]
        public void Create_WithValidData_ReturnsEvent()
        {
            // Arrange
            var applicationId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var eventType = EventType.PhoneScreen;
            var occurredAt = DateTime.UtcNow.AddDays(-1);
            var notes = "Great conversation with recruiter";

            // Act
            var @event = ApplicationEvent.Create(applicationId, accountId, eventType, occurredAt, notes);

            // Assert
            @event.Should().NotBeNull();
            @event.EventType.Should().Be(eventType);
            @event.OccurredAt.Should().Be(occurredAt);
            @event.Notes.Should().Be(notes);
        }

        [Fact]
        public void Create_WithFutureDate_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() =>
                ApplicationEvent.Create(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    EventType.Submitted,
                    DateTime.UtcNow.AddDays(1)
                )
            );
        }
    }
}
