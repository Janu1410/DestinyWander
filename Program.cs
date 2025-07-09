using System.Text;
using backend.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // Use AddControllers() for a Web API

builder.Services.AddScoped<IMongoDbService, MongoDbService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ITaxiBookingService, TaxiBookingService>();

// Configure HttpClient for AirScrapper API
builder.Services.AddHttpClient("AirScrapperClient", client =>
{
    var flightApiConfig = builder.Configuration.GetSection("AirScrapperApi");
    client.BaseAddress = new Uri(flightApiConfig["BaseUrl"]);
    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", flightApiConfig["ApiKey"]);
    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", flightApiConfig["ApiHost"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("TaxiClient", client =>
{
    var taxiApiConfig = builder.Configuration.GetSection("TaxiApi");
    client.BaseAddress = new Uri(taxiApiConfig["BaseUrl"]);
    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", taxiApiConfig["ApiKey"]);
    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", taxiApiConfig["ApiHost"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IFlightService, FlightService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("AirScrapperClient");
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<FlightService>>();
    return new FlightService(httpClient, configuration, logger);
});

builder.Services.AddScoped<ITaxiService, TaxiService>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("TaxiClient");
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<TaxiService>>();
    return new TaxiService(httpClient, configuration, logger);
});

// Google Authentication setup (disabled - secrets removed)
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = "Google";
// })
// .AddCookie(options =>
// {
//     options.LoginPath = "/Account/Login";
// })
// .AddGoogle("Google", options =>
// {
//     options.ClientId = "YOUR_GOOGLE_CLIENT_ID";
//     options.ClientSecret = "YOUR_GOOGLE_CLIENT_SECRET";
//     options.CallbackPath = "/signin-google";
// });

builder.Services.AddSession();

// Configure CORS to allow both frontend and Postman
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendAndPostman", builder =>
    {
        builder
            .WithOrigins("http://localhost:8080")
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"An unexpected error occurred.\"}");
        });
    });
    app.UseHsts();
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (app.Environment.IsDevelopment())
        {
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
            ctx.Context.Response.Headers["Expires"] = "0";
        }
    }
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontendAndPostman");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
});

app.Run();
