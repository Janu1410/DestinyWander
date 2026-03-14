using backend.Taxi.Models;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backend.Services
{

public interface ITaxiService
{
    Task<List<PlaceModel>> SearchLocationAsync(string query);
    Task<string> SearchTaxiAsync(TaxiSearchRequest request);
}

public class TaxiService : ITaxiService
{
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TaxiService> _logger;

    public TaxiService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TaxiService> logger
        )
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration =
                configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

   

    public async Task<List<PlaceModel>> SearchLocationAsync(string query)
{
    var requestUrl = $"api/v1/taxi/searchLocation?query={Uri.EscapeDataString(query)}&languagecode=en-gb";
    var response = await _httpClient.GetAsync(requestUrl);

    response.EnsureSuccessStatusCode();
    var content = await response.Content.ReadAsStringAsync();

    if (string.IsNullOrWhiteSpace(content))
    {
        return new List<PlaceModel>();
    }

    // Deserialize the JSON response into a JsonElement
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    // Parse the content into a JsonElement
    var jsonElement = JsonSerializer.Deserialize<JsonElement>(content, options);

    // Extract the 'Data' field from the JsonElement and deserialize it into a List<PlaceModel>
    if (jsonElement.TryGetProperty("data", out JsonElement dataElement))
    {
        var places = JsonSerializer.Deserialize<List<PlaceModel>>(dataElement.GetRawText(), options);
        return places ?? new List<PlaceModel>();
    }
    else
    {
        // If "Data" doesn't exist, return an empty list
        return new List<PlaceModel>();
    }
}

public async Task<string> SearchTaxiAsync(TaxiSearchRequest request)
{
    string url = $"api/v1/taxi/searchTaxi" +
                 $"?pick_up_place_id={Uri.EscapeDataString(request.PickUpPlaceId)}" +
                 $"&drop_off_place_id={Uri.EscapeDataString(request.DropOffPlaceId)}" +
                 $"&pick_up_date={Uri.EscapeDataString(request.PickUpDate)}" +
                 $"&pick_up_time={Uri.EscapeDataString(request.PickUpTime)}" +
                 $"&currency_code=INR" +
                 $"&languagecode=en-gb";

    try
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        

        return content;
    }
    catch (HttpRequestException ex)
    {
        Console.Error.WriteLine($"Request failed: {ex.Message}");
        return null;
    }
    catch (JsonException ex)
    {
        Console.Error.WriteLine($"JSON Deserialization failed: {ex.Message}");
        return null;
    }
}




   

}
}

