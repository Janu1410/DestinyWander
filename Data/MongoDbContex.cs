using MongoDB.Driver;
using backend.Hotel.Models;
using Microsoft.Extensions.Configuration;

namespace backend.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        
        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDb:ConnectionString"];
            var databaseName = configuration["MongoDb:DatabaseName"] ?? "TravelWebsiteDb";

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("MongoDB connection string is missing (MongoDb:ConnectionString).");
            }

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Booking> Bookings => _database.GetCollection<Booking>("HotelBookings");
    }
}
