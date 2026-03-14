using MongoDB.Driver;
using backend.Package.Models;
using Microsoft.Extensions.Configuration;

namespace backend.Package.Services
{
    public class BookingService
    {
        private readonly IMongoCollection<Booking> _bookings;

        public BookingService(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDb:ConnectionString"];
            var databaseName = configuration["MongoDb:DatabaseName"] ?? "TravelWebsiteDb";

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("MongoDB connection string is missing (MongoDb:ConnectionString).");
            }

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _bookings = database.GetCollection<Booking>("PackageBookings");   // 👉 Collection name
        }

        public void CreateBooking(Booking booking)
        {
            _bookings.InsertOne(booking);
        }
    }
}
