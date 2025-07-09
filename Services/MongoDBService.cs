using backend.Models;
using backend.Auth.Models;
using MongoDB.Driver;
using backend.Taxi.Models;

namespace backend.Services
{
    public interface IMongoDbService
    {
        IMongoCollection<UserViewModel> User { get; }
        IMongoCollection<FlightBookingModel> FlightBooking { get; }
        IMongoCollection<TaxiBooking> TaxiBooking { get; }
    }

    public class MongoDbService : IMongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDb:ConnectionString"]);
            _database = client.GetDatabase(configuration["MongoDb:DatabaseName"]);
        }

        public IMongoCollection<UserViewModel> User => _database.GetCollection<UserViewModel>("User");
        public IMongoCollection<FlightBookingModel> FlightBooking => _database.GetCollection<FlightBookingModel>("FlightBooking");
        public IMongoCollection<TaxiBooking> TaxiBooking => _database.GetCollection<TaxiBooking>("TaxiBooking");

        
    }
}
