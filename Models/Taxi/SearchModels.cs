namespace backend.Taxi.Models;


     public class TaxiSearchRequest
    {
        public string PickUpPlaceId { get; set; }
        public string DropOffPlaceId { get; set; }
        public string PickUpDate { get; set; }  // You can use DateTime if you prefer
        public string PickUpTime { get; set; }  // Same here, if you prefer TimeSpan
    }


  public class TaxiModel
{
    public List<Journey> Journeys { get; set; }
    public List<Result> Results { get; set; }
}

public class Journey
{
    public string JourneyDirection { get; set; }
    public Location DropOffLocation { get; set; }
    public Location PickupLocation { get; set; }
    public DateTime RequestedPickupDateTime { get; set; }
    public string JanusSearchReference { get; set; }
}

public class Location
{
    public string LocationId { get; set; }
    public string Establishment { get; set; }
    public string LocationType { get; set; }
    public string AirportCode { get; set; }  // Nullable as it can be null
    public string Name { get; set; }
    public bool IsRideNowAvailable { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public LatLng LatLng { get; set; }
    public string Description { get; set; }
    public string Postcode { get; set; }
}

public class LatLng
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class Result
{
    public string JanusResultReference { get; set; }
    public string CategoryLocalised { get; set; }
    public Price Price { get; set; }
    public double DrivingDistance { get; set; }
    public bool MeetGreet { get; set; }
    public bool NonRefundable { get; set; }
    public string SupplierName { get; set; }
    public string Category { get; set; }
    public string ResultId { get; set; }
    public int CancellationLeadTimeMinutes { get; set; }
    public int PassengerCapacity { get; set; }
    public string ImageUrl { get; set; }
    public int PriceRuleId { get; set; }
    public int SupplierId { get; set; }
    public string VehicleType { get; set; }
    public int Bags { get; set; }
    public int Duration { get; set; }
    public string Description { get; set; }
    public List<PriceBreakdown> PriceBreakdown { get; set; }
}

public class Price
{
    public string CurrencyCode { get; set; }
    public decimal Amount { get; set; }
}

public class PriceBreakdown
{
    public string DiscountType { get; set; }
    public bool GeniusDiscount { get; set; }
    public List<LegPriceBreakdown> LegPriceBreakdown { get; set; }
}

public class LegPriceBreakdown
{
    public string JourneyDirection { get; set; }
    public string SupplierName { get; set; }
    public Price Price { get; set; }
    public int SupplierId { get; set; }
    public int SupplierLocationId { get; set; }
    public string DescriptionLocalised { get; set; }
}
