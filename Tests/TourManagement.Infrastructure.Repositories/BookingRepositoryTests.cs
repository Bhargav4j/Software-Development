using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TourManagement.Infrastructure.Repositories;
using TourManagement.Infrastructure.Data;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Repositories.Tests
{
    public class BookingRepositoryTests
    {
        private readonly Mock<ILogger<BookingRepository>> _mockLogger;
        private readonly DbContextOptions<TourManagementDbContext> _dbContextOptions;

        public BookingRepositoryTests()
        {
            _mockLogger = new Mock<ILogger<BookingRepository>>();
            _dbContextOptions = new DbContextOptionsBuilder<TourManagementDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private TourManagementDbContext CreateContext()
        {
            return new TourManagementDbContext(_dbContextOptions);
        }

        [Fact]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BookingRepository(null!, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            using var context = CreateContext();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new BookingRepository(context, null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange
            using var context = CreateContext();

            // Act
            var repository = new BookingRepository(context, _mockLogger.Object);

            // Assert
            Assert.NotNull(repository);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyActiveBookings()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.Bookings.AddRange(
                new Booking { Id = 1, TourId = 1, UserId = 1, IsActive = true },
                new Booking { Id = 2, TourId = 1, UserId = 1, IsActive = true },
                new Booking { Id = 3, TourId = 1, UserId = 1, IsActive = false }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, booking => Assert.True(booking.IsActive));
        }

        [Fact]
        public async Task GetAllAsync_IncludesTourAndUser()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.Bookings.Add(new Booking { Id = 1, TourId = 1, UserId = 1, IsActive = true });
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var booking = result.First();
            Assert.NotNull(booking.Tour);
            Assert.NotNull(booking.User);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingActiveId_ReturnsBooking()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var booking = new Booking { Id = 1, TourId = 1, UserId = 1, IsActive = true };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WithInactiveId_ReturnsNull()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var booking = new Booking { Id = 1, TourId = 1, UserId = 1, IsActive = false };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_WithExistingUserId_ReturnsUserBookings()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            var user1 = new User { Id = 1, Email = "user1@example.com", IsActive = true };
            var user2 = new User { Id = 2, Email = "user2@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();

            context.Bookings.AddRange(
                new Booking { Id = 1, TourId = 1, UserId = 1, IsActive = true },
                new Booking { Id = 2, TourId = 1, UserId = 1, IsActive = true },
                new Booking { Id = 3, TourId = 1, UserId = 2, IsActive = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByUserIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, booking => Assert.Equal(1, booking.UserId));
        }

        [Fact]
        public async Task GetByUserIdAsync_OnlyReturnsActiveBookings()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.Bookings.AddRange(
                new Booking { Id = 1, TourId = 1, UserId = 1, IsActive = true },
                new Booking { Id = 2, TourId = 1, UserId = 1, IsActive = false }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByUserIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.All(result, booking => Assert.True(booking.IsActive));
        }

        [Fact]
        public async Task GetByTourIdAsync_WithExistingTourId_ReturnsTourBookings()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour1 = new Tour { Id = 1, TourName = "Tour 1", IsActive = true };
            var tour2 = new Tour { Id = 2, TourName = "Tour 2", IsActive = true };
            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Tours.AddRange(tour1, tour2);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.Bookings.AddRange(
                new Booking { Id = 1, TourId = 1, UserId = 1, IsActive = true },
                new Booking { Id = 2, TourId = 1, UserId = 1, IsActive = true },
                new Booking { Id = 3, TourId = 2, UserId = 1, IsActive = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByTourIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, booking => Assert.Equal(1, booking.TourId));
        }

        [Fact]
        public async Task GetByTourIdAsync_OnlyReturnsActiveBookings()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            context.Bookings.AddRange(
                new Booking { Id = 1, TourId = 1, UserId = 1, IsActive = true },
                new Booking { Id = 2, TourId = 1, UserId = 1, IsActive = false }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetByTourIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.All(result, booking => Assert.True(booking.IsActive));
        }

        [Fact]
        public async Task AddAsync_WithValidBooking_AddsBookingToDatabase()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var booking = new Booking { TourId = 1, UserId = 1, IsActive = true };

            // Act
            var result = await repository.AddAsync(booking);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);

            var savedBooking = await context.Bookings.FindAsync(result.Id);
            Assert.NotNull(savedBooking);
            Assert.Equal(1, savedBooking.TourId);
            Assert.Equal(1, savedBooking.UserId);
        }

        [Fact]
        public async Task UpdateAsync_WithExistingBooking_UpdatesBooking()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var booking = new Booking { TourId = 1, UserId = 1, NumberOfPersons = 2, IsActive = true };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();
            context.Entry(booking).State = EntityState.Detached;

            // Act
            booking.NumberOfPersons = 5;
            await repository.UpdateAsync(booking);

            // Assert
            var updatedBooking = await context.Bookings.FindAsync(booking.Id);
            Assert.NotNull(updatedBooking);
            Assert.Equal(5, updatedBooking.NumberOfPersons);
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_SetsIsActiveToFalse()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var booking = new Booking { Id = 1, TourId = 1, UserId = 1, IsActive = true };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Act
            await repository.DeleteAsync(1);

            // Assert
            var deletedBooking = await context.Bookings.FindAsync(1);
            Assert.NotNull(deletedBooking);
            Assert.False(deletedBooking.IsActive);
            Assert.NotNull(deletedBooking.ModifiedDate);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_DoesNotThrowException()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            // Act & Assert
            await repository.DeleteAsync(999); // Should not throw
        }

        [Fact]
        public async Task DeleteAsync_SetsModifiedDateToUtcNow()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var booking = new Booking { Id = 1, TourId = 1, UserId = 1, IsActive = true };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();
            var beforeDelete = DateTime.UtcNow.AddSeconds(-1);

            // Act
            await repository.DeleteAsync(1);
            var afterDelete = DateTime.UtcNow.AddSeconds(1);

            // Assert
            var deletedBooking = await context.Bookings.FindAsync(1);
            Assert.NotNull(deletedBooking);
            Assert.NotNull(deletedBooking.ModifiedDate);
            Assert.True(deletedBooking.ModifiedDate >= beforeDelete && deletedBooking.ModifiedDate <= afterDelete);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingActiveId_ReturnsTrue()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var booking = new Booking { Id = 1, TourId = 1, UserId = 1, IsActive = true };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.ExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithInactiveId_ReturnsFalse()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            var tour = new Tour { Id = 1, TourName = "Test Tour", IsActive = true };
            var user = new User { Id = 1, Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var booking = new Booking { Id = 1, TourId = 1, UserId = 1, IsActive = false };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.ExistsAsync(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistentId_ReturnsFalse()
        {
            // Arrange
            using var context = CreateContext();
            var repository = new BookingRepository(context, _mockLogger.Object);

            // Act
            var result = await repository.ExistsAsync(999);

            // Assert
            Assert.False(result);
        }
    }
}
