
namespace backend.Taxi.Models;

public class PlaceModel
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Country { get; set; }
    public string GooglePlaceId { get; set; }
    public string CountryCode { get; set; }
    public string Iata { get; set; }
    public string Types { get; set; }
    public string City { get; set; }
    public string? Description { get; set; }  // nullable
    public string Name { get; set; }
}
