using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace backend.Taxi.Models;

public class TaxiBooking{
     [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } // MongoDB document ID

        [Required]
        public string ResultId { get; set; }

        [Required]
        public string SupplierName {get; set;}

        [Required]
        public string VehicleType {get; set;}

        [Required]
        public string Duration {get; set;}

        [Required]
        public string PickUpDateTime {get; set;}

        [Required]
        public string DropOffLocation {get; set;}

        [Required]
        public string PickupLocation { get; set; }

        [Required]
        public string PriceAmount { get; set;}

        [BsonIgnoreIfNull]
        [JsonIgnore]
        public string? UserId { get; set; }

        [BsonIgnoreIfNull]
        [JsonIgnore]
        public string? PaymentStatus { get; set; } = "Pending";
}