

using backend.Taxi.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace backend.Controllers ;

[Route("api/[controller]")]
[ApiController]
public class TaxiController : Controller
{
    private readonly ILogger<TaxiController> _logger;
    private readonly ITaxiService _taxiService;
    private readonly ITaxiBookingService _taxiBookingService;

    public TaxiController(ITaxiService taxiService, ILogger<TaxiController> logger , ITaxiBookingService taxiBookingService)
    {
        _taxiService = taxiService;
         _taxiBookingService = taxiBookingService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index(){
        return View("~/Views/Taxi/Index.cshtml");
    }

    [HttpGet("search")]
    public async Task<List<PlaceModel>> SearchTaxis(string query)
    {
        try
        {
           List<PlaceModel> availableTaxis = await _taxiService.SearchLocationAsync(query);
            return availableTaxis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for taxis");
             return new List<PlaceModel>();
        }
    }

    [HttpPost("get-details")]
    public async Task<IActionResult> GetDetailsOfTaxi([FromBody] TaxiSearchRequest model)
    {
        try
        {
        var taxi = await _taxiService.SearchTaxiAsync(model);
            return Ok(taxi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for taxis");
             return null;
        }
    }

    [HttpGet("payment")]
    public  async Task<IActionResult> Payment(string bookingId){
        var model = await _taxiBookingService.GetTaxiBookingByIdAsync(bookingId);
        return  View("~/Views/Taxi/Payment.cshtml" , model);
    }

    [HttpGet("success")]
    public  async Task<IActionResult> Confirmation(string bookingId){
        var model = await _taxiBookingService.GetTaxiBookingByIdAsync(bookingId);
        model.PaymentStatus = "confirmed";
        return  View("~/Views/Taxi/Confirmation.cshtml" , model);
    }
    
    

    
    [Authorize]
[HttpPost("book")]
public async Task<IActionResult> BookTaxi([FromBody] TaxiBooking model)
{
    try
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim != null)
        {
            model.UserId = userIdClaim.Value;
        }
        else
        {
            model.UserId = string.Empty;
        }

        // Save booking
        await _taxiBookingService.SaveTaxiBookingAsync(model);

        // After saving, make sure model.Id is available
        if (string.IsNullOrEmpty(model.Id))
        {
            // Handle error properly if saving failed
            return StatusCode(500, new { message = "Booking failed. Could not generate booking Id." });
        }

        // Return JSON with redirect URL
        
       return Ok(
    new { redirectUrl = Url.Action("Payment", "Taxi", new { bookingId = model.Id }) }
);

    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error booking taxi");
        return StatusCode(500, new { message = "An error occurred while booking the taxi" });
    }
}


}
