using MongoDB.Driver;
using backend.Package.Models;

namespace backend.Package.Services
{
    public class BookingService
    {
        private readonly IMongoCollection<Booking> _bookings;

        public BookingService()
        {
            var client = new MongoClient("mongodb://localhost:27017"); // 👉 MongoDB connection string
            var database = client.GetDatabase("TravelWebsiteDb");             // 👉 Database name
            _bookings = database.GetCollection<Booking>("PackageBookings");   // 👉 Collection name
        }

        public void CreateBooking(Booking booking)
        {
            _bookings.InsertOne(booking);
        }
    }
}
