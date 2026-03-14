using Microsoft.AspNetCore.Mvc;
using backend.Auth.Models;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [AllowAnonymous]
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

                // Save user to MongoDB with hashed password
                var userToCreate = new UserViewModel
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    AgreeToTerms = model.AgreeToTerms
                };

                _context.User.InsertOne(userToCreate);  

                // Notify the user about successful account creation
                TempData["Success"] = "Account created successfully! Please log in.";
                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = _context.User.Find(u => u.Email == model.Email).FirstOrDefault();

                if (user != null)
                {
                    // Support both hashed and legacy plain-text passwords.
                    var isPasswordValid = false;
                    var storedPassword = user.Password ?? string.Empty;
                    var looksHashed = storedPassword.StartsWith("$2a$") ||
                                      storedPassword.StartsWith("$2b$") ||
                                      storedPassword.StartsWith("$2y$");

                    if (looksHashed)
                    {
                        isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, storedPassword);
                    }
                    else
                    {
                        isPasswordValid = string.Equals(storedPassword, model.Password, StringComparison.Ordinal);

                        // Upgrade legacy account to hashed password on successful login.
                        if (isPasswordValid)
                        {
                            var newHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                            var update = Builders<UserViewModel>.Update.Set(u => u.Password, newHash);
                            _context.User.UpdateOne(u => u.Id == user.Id, update);
                        }
                    }

                    if (!isPasswordValid)
                    {
                        ModelState.AddModelError("", "Invalid email or password");
                        return View(model);
                    }

                    // Create Claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.FirstName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("UserId", user.Id ?? string.Empty)
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddHours(8) : null
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        authProperties);

                    // Store session
                    HttpContext.Session.SetString("UserEmail", user.Email);

                    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid email or password");
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("GoogleResponse", new { returnUrl });
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GoogleResponse(string? returnUrl = null)
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
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // Optional: Clear session
            return RedirectToAction("Index", "Home");
        }
    }
}
