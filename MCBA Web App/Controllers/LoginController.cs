using MCBA_Web_App.Data;  // Import the data access layer for interaction with the database.
using MCBA_Web_App.Models;  // Import model classes such as Login and Customer.
using Microsoft.AspNetCore.Mvc;  // Import ASP.NET Core MVC framework.
using SimpleHashing.Net;  // Import SimpleHashing library for password hashing and verification.
using System.Runtime.CompilerServices;  // Import for compiler-related attributes (not used explicitly here).
using Microsoft.AspNetCore.Http;  // Import ASP.NET Core HTTP context for session management.

namespace MCBA_Web_App.Controllers
{
    // This controller handles login and logout functionality. The custom route "/SecureLogin" maps to this controller.
    [Route("/SecureLogin")]
    public class LoginController : Controller
    {
        // A static instance of ISimpleHash is used for password hashing and verification.
        private static readonly ISimpleHash s_simpleHash = new SimpleHash();

        // Database context to interact with the MCBA database.
        private readonly MCBAContext _context;

        // Constructor to inject the MCBAContext dependency.
        public LoginController(MCBAContext context) => _context = context;

        // Action to display the login view when a GET request is made to the login page.
        public IActionResult Login() => View();

        // POST action to handle login attempts. Accepts login credentials as parameters.
        [HttpPost]
        public async Task<IActionResult> Login(string loginID, string password)
        {
            // Attempt to find the login entry in the database based on the loginID.
            var login = await _context.Login.FindAsync(loginID);

            // Check if login is null, the password is empty, or if the password verification fails.
            if (login == null || string.IsNullOrEmpty(password) || !s_simpleHash.Verify(password, login.PasswordHash))
            {
                // If login fails, add an error message to the ModelState to inform the user.
                ModelState.AddModelError("LoginFailed", "Login failed, please try again.");

                // Return the login view with the LoginID field populated so the user doesn't need to re-enter it.
                return View(new Login { LoginID = loginID });
            }

            // Retrieve the CustomerID associated with the successful login.
            var custID = login.CustomerID;

            // Find the customer in the database using the CustomerID.
            var customer = await _context.Customer.FindAsync(custID);

            // Set the CustomerID and Name in the session to maintain the login session.
            HttpContext.Session.SetInt32(nameof(Customer.CustomerID), login.CustomerID);
            HttpContext.Session.SetString(nameof(Customer.Name), customer.Name);

            // Redirect the logged-in customer to the landing page (Index action in the LandingController).
            return RedirectToAction("Index", "Landing");
        }

        // Action to log the user out. This route is explicitly named "LogoutNow".
        [Route("LogoutNow")]
        public IActionResult Logout()
        {
            // Clear the session, effectively logging out the customer.
            HttpContext.Session.Clear();

            // Redirect the user to the home page after logging out.
            return RedirectToAction("Index", "Home");
        }
    }
}
