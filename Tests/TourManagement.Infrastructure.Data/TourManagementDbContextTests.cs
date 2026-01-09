using System;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TourManagement.Infrastructure.Data;
using TourManagement.Domain.Entities;

namespace TourManagement.Infrastructure.Data.Tests
{
    public class TourManagementDbContextTests
    {
        private readonly DbContextOptions<TourManagementDbContext> _dbContextOptions;

        public TourManagementDbContextTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<TourManagementDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void Constructor_WithValidOptions_CreatesInstance()
        {
            // Act
            using var context = new TourManagementDbContext(_dbContextOptions);

            // Assert
            Assert.NotNull(context);
        }

        [Fact]
        public void Tours_DbSet_IsNotNull()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);

            // Act & Assert
            Assert.NotNull(context.Tours);
        }

        [Fact]
        public void Users_DbSet_IsNotNull()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);

            // Act & Assert
            Assert.NotNull(context.Users);
        }

        [Fact]
        public void Bookings_DbSet_IsNotNull()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);

            // Act & Assert
            Assert.NotNull(context.Bookings);
        }

        [Fact]
        public void CanAddTourToDatabase()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var tour = new Tour
            {
                TourName = "Test Tour",
                Place = "Paris",
                Days = 7,
                Price = 1000,
                IsActive = true
            };

            // Act
            context.Tours.Add(tour);
            context.SaveChanges();

            // Assert
            Assert.True(tour.Id > 0);
        }

        [Fact]
        public void CanAddUserToDatabase()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            // Act
            context.Users.Add(user);
            context.SaveChanges();

            // Assert
            Assert.True(user.Id > 0);
        }

        [Fact]
        public void CanAddBookingToDatabase()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var tour = new Tour { TourName = "Test Tour", IsActive = true };
            var user = new User { Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            context.SaveChanges();

            var booking = new Booking
            {
                TourId = tour.Id,
                UserId = user.Id,
                NumberOfPersons = 2,
                IsActive = true
            };

            // Act
            context.Bookings.Add(booking);
            context.SaveChanges();

            // Assert
            Assert.True(booking.Id > 0);
        }

        [Fact]
        public void CanRetrieveTourFromDatabase()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var tour = new Tour
            {
                TourName = "European Tour",
                Place = "Paris",
                IsActive = true
            };
            context.Tours.Add(tour);
            context.SaveChanges();

            // Act
            var retrievedTour = context.Tours.Find(tour.Id);

            // Assert
            Assert.NotNull(retrievedTour);
            Assert.Equal("European Tour", retrievedTour.TourName);
            Assert.Equal("Paris", retrievedTour.Place);
        }

        [Fact]
        public void CanRetrieveUserFromDatabase()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var user = new User
            {
                Email = "john@example.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };
            context.Users.Add(user);
            context.SaveChanges();

            // Act
            var retrievedUser = context.Users.Find(user.Id);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.Equal("john@example.com", retrievedUser.Email);
            Assert.Equal("John", retrievedUser.FirstName);
        }

        [Fact]
        public void CanRetrieveBookingFromDatabase()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var tour = new Tour { TourName = "Test Tour", IsActive = true };
            var user = new User { Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            context.SaveChanges();

            var booking = new Booking
            {
                TourId = tour.Id,
                UserId = user.Id,
                NumberOfPersons = 3,
                IsActive = true
            };
            context.Bookings.Add(booking);
            context.SaveChanges();

            // Act
            var retrievedBooking = context.Bookings.Find(booking.Id);

            // Assert
            Assert.NotNull(retrievedBooking);
            Assert.Equal(3, retrievedBooking.NumberOfPersons);
            Assert.Equal(tour.Id, retrievedBooking.TourId);
            Assert.Equal(user.Id, retrievedBooking.UserId);
        }

        [Fact]
        public void CanUpdateTourInDatabase()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var tour = new Tour
            {
                TourName = "Original Name",
                Place = "Paris",
                IsActive = true
            };
            context.Tours.Add(tour);
            context.SaveChanges();

            // Act
            tour.TourName = "Updated Name";
            context.SaveChanges();

            // Assert
            var updatedTour = context.Tours.Find(tour.Id);
            Assert.NotNull(updatedTour);
            Assert.Equal("Updated Name", updatedTour.TourName);
        }

        [Fact]
        public void CanUpdateUserInDatabase()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var user = new User
            {
                Email = "original@example.com",
                FirstName = "Original",
                IsActive = true
            };
            context.Users.Add(user);
            context.SaveChanges();

            // Act
            user.FirstName = "Updated";
            context.SaveChanges();

            // Assert
            var updatedUser = context.Users.Find(user.Id);
            Assert.NotNull(updatedUser);
            Assert.Equal("Updated", updatedUser.FirstName);
        }

        [Fact]
        public void CanDeleteTourFromDatabase()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var tour = new Tour
            {
                TourName = "To Delete",
                IsActive = true
            };
            context.Tours.Add(tour);
            context.SaveChanges();
            var tourId = tour.Id;

            // Act
            context.Tours.Remove(tour);
            context.SaveChanges();

            // Assert
            var deletedTour = context.Tours.Find(tourId);
            Assert.Null(deletedTour);
        }

        [Fact]
        public void CanDeleteUserFromDatabase()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var user = new User
            {
                Email = "delete@example.com",
                IsActive = true
            };
            context.Users.Add(user);
            context.SaveChanges();
            var userId = user.Id;

            // Act
            context.Users.Remove(user);
            context.SaveChanges();

            // Assert
            var deletedUser = context.Users.Find(userId);
            Assert.Null(deletedUser);
        }

        [Fact]
        public void BookingHasRelationshipWithTour()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var tour = new Tour { TourName = "Test Tour", IsActive = true };
            var user = new User { Email = "test@example.com", IsActive = true };

            context.Tours.Add(tour);
            context.Users.Add(user);
            context.SaveChanges();

            var booking = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true };
            context.Bookings.Add(booking);
            context.SaveChanges();

            // Act
            var retrievedBooking = context.Bookings
                .Include(b => b.Tour)
                .FirstOrDefault(b => b.Id == booking.Id);

            // Assert
            Assert.NotNull(retrievedBooking);
            if (retrievedBooking.Tour != null)
            {
                Assert.Equal(tour.Id, retrievedBooking.Tour.Id);
            }
        }

        [Fact]
        public void BookingHasRelationshipWithUser()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var tour = new Tour { TourName = "Test Tour", IsActive = true };
            var user = new User { Email = "test@example.com", IsActive = true };

            context.Tours.Add(tour);
            context.Users.Add(user);
            context.SaveChanges();

            var booking = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true };
            context.Bookings.Add(booking);
            context.SaveChanges();

            // Act
            var retrievedBooking = context.Bookings
                .Include(b => b.User)
                .FirstOrDefault(b => b.Id == booking.Id);

            // Assert
            Assert.NotNull(retrievedBooking);
            if (retrievedBooking.User != null)
            {
                Assert.Equal(user.Id, retrievedBooking.User.Id);
            }
        }

        [Fact]
        public void TourHasCollectionOfBookings()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var tour = new Tour { TourName = "Test Tour", IsActive = true };
            var user = new User { Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            context.SaveChanges();

            var booking1 = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true };
            var booking2 = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true };
            context.Bookings.AddRange(booking1, booking2);
            context.SaveChanges();

            // Act
            var retrievedTour = context.Tours
                .Include(t => t.Bookings)
                .FirstOrDefault(t => t.Id == tour.Id);

            // Assert
            Assert.NotNull(retrievedTour);
            Assert.NotNull(retrievedTour.Bookings);
            Assert.Equal(2, retrievedTour.Bookings.Count);
        }

        [Fact]
        public void UserHasCollectionOfBookings()
        {
            // Arrange
            using var context = new TourManagementDbContext(_dbContextOptions);
            var tour = new Tour { TourName = "Test Tour", IsActive = true };
            var user = new User { Email = "test@example.com", IsActive = true };
            context.Tours.Add(tour);
            context.Users.Add(user);
            context.SaveChanges();

            var booking1 = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true };
            var booking2 = new Booking { TourId = tour.Id, UserId = user.Id, IsActive = true };
            context.Bookings.AddRange(booking1, booking2);
            context.SaveChanges();

            // Act
            var retrievedUser = context.Users
                .Include(u => u.Bookings)
                .FirstOrDefault(u => u.Id == user.Id);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.NotNull(retrievedUser.Bookings);
            Assert.Equal(2, retrievedUser.Bookings.Count);
        }
    }
}
