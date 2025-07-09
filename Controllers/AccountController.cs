using Microsoft.AspNetCore.Mvc;
using backend.Auth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.Google;
using backend.Services;

namespace backend.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMongoDbService _context;
        private readonly ILogger<AccountController> _logger ;

        public AccountController(IMongoDbService context , ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger ;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Ensure that the user has agreed to the terms and conditions
                if (!model.AgreeToTerms)
                {
                    ModelState.AddModelError("AgreeToTerms", "You must agree to the terms and conditions.");
                    return View(model);
                }

                // Check if user already exists
                var existing = _context.User.Find(u => u.Email == model.Email).FirstOrDefault();
                if (existing != null)
                {
                    ModelState.AddModelError("Email", "User already exists.");
                    return View(model);
                }

                // Save user to MongoDB
                _context.User.InsertOne(model);  

                // Notify the user about successful account creation
                TempData["Success"] = "Account created successfully! Please log in.";
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.User.Find(u => u.Email == model.Email && u.Password == model.Password).FirstOrDefault();

                if (user != null)
                {
                    // Create Claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.FirstName),
                        new Claim(ClaimTypes.Email, user.Email)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // Store session
                    HttpContext.Session.SetString("UserEmail", user.Email);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid email or password");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return RedirectToAction("Login");

            var claims = authenticateResult.Principal.Identities
                .FirstOrDefault()?.Claims.Select(claim => new
                {
                    claim.Type,
                    claim.Value
                });

            // Example: You can fetch user's name, email from claims
            var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;

            // You can now save this user info into your MongoDB Users collection if you want.

            // After login, redirect to Homepage or Dashboard
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // Optional: Clear session
            return RedirectToAction("Index", "Home");
        }
    }
}