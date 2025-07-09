using MongoDB.Driver;
using backend.Hotel.Models;

namespace backend.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        
        public MongoDbContext()
        {
            // Replace this with your MongoDB connection string
            var client = new MongoClient("mongodb://localhost:27017"); 
            _database = client.GetDatabase("TravelWebsiteDb");
        }

        public IMongoCollection<Booking> Bookings => _database.GetCollection<Booking>("HotelBookings");
    }
}
