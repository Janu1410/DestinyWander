using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Hotel.Models
{
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("HotelId")]
        public string HotelId { get; set; } = string.Empty;  

        [BsonElement("Name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("Phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("Email")]
        public string Email { get; set; } = string.Empty;
    }
}
