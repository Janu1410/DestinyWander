using System.Text;
using backend.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ===================== Services =====================

builder.Services.AddControllersWithViews();

builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);

// -------- Mongo & App Services --------
builder.Services.AddScoped<IMongoDbService, MongoDbService>();
builder.Services.AddScoped<backend.Data.MongoDbContext>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ITaxiBookingService, TaxiBookingService>();
builder.Services.AddScoped<backend.Package.Services.BookingService>();

// -------- HttpClient: AirScrapper --------
builder.Services.AddHttpClient("AirScrapperClient", client =>
{
    var flightApiConfig = builder.Configuration.GetSection("AirScrapperApi");

    var baseUrl = flightApiConfig["BaseUrl"];
    if (!string.IsNullOrEmpty(baseUrl))
        client.BaseAddress = new Uri(baseUrl);

    if (!string.IsNullOrEmpty(flightApiConfig["ApiKey"]))
        client.DefaultRequestHeaders.Add("X-RapidAPI-Key", flightApiConfig["ApiKey"]);

    if (!string.IsNullOrEmpty(flightApiConfig["ApiHost"]))
        client.DefaultRequestHeaders.Add("X-RapidAPI-Host", flightApiConfig["ApiHost"]);

    client.Timeout = TimeSpan.FromSeconds(30);
});

// -------- HttpClient: Taxi --------
builder.Services.AddHttpClient("TaxiClient", client =>
{
    var taxiApiConfig = builder.Configuration.GetSection("TaxiApi");

    var baseUrl = taxiApiConfig["BaseUrl"];
    if (!string.IsNullOrEmpty(baseUrl))
        client.BaseAddress = new Uri(baseUrl);

    if (!string.IsNullOrEmpty(taxiApiConfig["ApiKey"]))
        client.DefaultRequestHeaders.Add("X-RapidAPI-Key", taxiApiConfig["ApiKey"]);

    if (!string.IsNullOrEmpty(taxiApiConfig["ApiHost"]))
        client.DefaultRequestHeaders.Add("X-RapidAPI-Host", taxiApiConfig["ApiHost"]);

    client.Timeout = TimeSpan.FromSeconds(30);
});

// -------- Custom Services --------
builder.Services.AddScoped<IFlightService, FlightService>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("AirScrapperClient");
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

// -------- Hotel Service --------
builder.Services.AddScoped<backend.Hotel.Services.IHotelService, backend.Hotel.Services.HotelService>();

// -------- Session --------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// -------- CORS --------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendAndPostman", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// -------- Authentication (Cookie + JWT) --------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new Exception("JWT Key is missing in configuration (Jwt:Key).");
}

var key = Encoding.UTF8.GetBytes(jwtKey);

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            var acceptsHtml = context.Request.Headers.Accept.Any(h =>
                h.Contains("text/html", StringComparison.OrdinalIgnoreCase));

            if (!acceptsHtml)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            if (!HttpMethods.IsGet(context.Request.Method))
            {
                context.Response.Redirect("/Account/Login");
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            var acceptsHtml = context.Request.Headers.Accept.Any(h =>
                h.Contains("text/html", StringComparison.OrdinalIgnoreCase));

            if (!acceptsHtml)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            if (!HttpMethods.IsGet(context.Request.Method))
            {
                context.Response.Redirect("/Account/Login");
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
    };
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Add Google only when real credentials are configured.
var googleClientId = builder.Configuration["Google:ClientId"];
var googleClientSecret = builder.Configuration["Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) &&
    !string.IsNullOrWhiteSpace(googleClientSecret) &&
    !googleClientId.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

// -------- Logging --------
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// ===================== Middleware =====================

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

// app.UseHttpsRedirection(); // enable in prod

app.UseRouting();

app.UseCors("AllowFrontendAndPostman");

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
