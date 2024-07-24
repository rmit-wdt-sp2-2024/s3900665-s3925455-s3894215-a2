using MCBA_Web_App.Data;
using MCBA_Web_App.Models;
using Microsoft.AspNetCore.Mvc;
using SimpleHashing.Net;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;

namespace MCBA_Web_App.Controllers
{
    [Route("/SecureLogin")]
    public class LoginController : Controller
    {
        private static readonly ISimpleHash s_simpleHash = new SimpleHash();

        private readonly MCBAContext _context;

        public LoginController(MCBAContext context) => _context = context;

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string loginID, string password)
        {
            var login = await _context.Login.FindAsync(loginID);

            if (login == null || string.IsNullOrEmpty(password) || !s_simpleHash.Verify(password, login.PasswordHash))
            {
                ModelState.AddModelError("LoginFailed", "Login failed, please try again.");
                return View(new Login { LoginID = loginID });
            }
            var custID = login.CustomerID;
            var customer = await _context.Customer.FindAsync(custID);

            // Login customer.
            HttpContext.Session.SetInt32(nameof(Customer.CustomerID), login.CustomerID);
            HttpContext.Session.SetString(nameof(Customer.Name), customer.Name);

            return RedirectToAction("Index", "Landing");
        }

        [Route("LogoutNow")]
        public IActionResult Logout()
        {
            // Logout customer.
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}
