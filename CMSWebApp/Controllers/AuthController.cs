using CMSWebApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CMSWebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly CmsdbContext _context;

        public AuthController(CmsdbContext context)
        {
            _context = context;
        }
        // GET: User login
        public IActionResult login()
        {
            return View();
        }
        // POST: User login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User user)
        {
            if (!string.IsNullOrEmpty(user.UserName) && !string.IsNullOrEmpty(user.Password))
            {
                User userData = await _context
                                      .Users
                                      .Where(u => u.UserName == user.UserName && u.Password == user.Password)
                                      .FirstOrDefaultAsync();

                ClaimsIdentity identity = new();
                bool isAuthenticated = false;

                if (userData != null)
                {
                    identity = CreateClaimsIdentity(userData);
                    isAuthenticated = true;
                }

                if (isAuthenticated)
                {
                    var principal = new ClaimsPrincipal(identity);

                    // Store the user info to Session
                    var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("Index", "Users");
                }
            }
            return View(user);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            var login = HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("login");
        }

        private static ClaimsIdentity CreateClaimsIdentity(User user)
        {
            var userID = user.Id.ToString();

            var claims = new Claim[]
            {
                new Claim("ct:CustomClaim:UserID", userID),
                new Claim("ct:CustomClaim:UserName", user.UserName),
                new Claim("ct:CustomClaim:Email", user.Email),
            };

            return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
