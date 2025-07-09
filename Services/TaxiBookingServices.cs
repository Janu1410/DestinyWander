using backend.Taxi.Models;
using MongoDB.Driver;

namespace backend.Services
{
    public interface ITaxiBookingService
    {
        Task SaveTaxiBookingAsync(TaxiBooking booking);
        Task<TaxiBooking> GetTaxiBookingByIdAsync(string id);
        Task UpdateTaxiBookingAsync(string bookingId, TaxiBooking updatedBooking);
    }

    public class TaxiBookingService : ITaxiBookingService
    {
        private readonly IMongoCollection<TaxiBooking> _taxiBookingCollection;

        public TaxiBookingService(IMongoDbService mongoDbService)
        {
            _taxiBookingCollection = mongoDbService.TaxiBooking;
        }

        public async Task SaveTaxiBookingAsync(TaxiBooking booking)
        {
            await _taxiBookingCollection.InsertOneAsync(booking);
        }

        public async Task<TaxiBooking> GetTaxiBookingByIdAsync(string id)
        {
            return await _taxiBookingCollection.Find(b => b.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateTaxiBookingAsync(string bookingId, TaxiBooking updatedBooking)
        {
            await _taxiBookingCollection.ReplaceOneAsync(
                booking => booking.Id == bookingId,
                updatedBooking
            );
        }
    }
}
