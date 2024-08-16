using MCBA_Web_App.Data;  // Imports the data layer for interacting with the database.
using MCBA_Web_App.Models;  // Imports model classes such as Login and Customer.
using Microsoft.AspNetCore.Mvc;  // Imports ASP.NET Core MVC for creating controllers and handling requests.
using SimpleHashing.Net;  // Imports the SimpleHashing library for securely hashing and verifying passwords.
using System.Runtime.CompilerServices;  // Imports compiler services, though not used explicitly in the code.
using Microsoft.AspNetCore.Http;  // Imports ASP.NET Core HTTP context, enabling session management.

namespace MCBA_Web_App.Controllers
{
    // Defines the route for the controller, so all actions within this controller are accessible under the "/SecureLogin" path.
    [Route("/SecureLogin")]
    public class LoginController : Controller
    {
        // Static field to hold the hashing implementation. Using `SimpleHash` from SimpleHashing.Net for password verification.
        private static readonly ISimpleHash s_simpleHash = new SimpleHash();

        // Private field for database context to interact with the MCBA database.
        private readonly MCBAContext _context;

        // Constructor that injects the MCBAContext into the controller. This allows the controller to access the database.
        public LoginController(MCBAContext context) => _context = context;

        // Action method to display the login form. This is invoked when the user navigates to the login page (GET request).
        public IActionResult Login() => View();

        // Action method to process the login form submission (POST request). It takes the user's `loginID` and `password` as inputs.
        [HttpPost]
        public async Task<IActionResult> Login(string loginID, string password)
        {
            // Attempt to find the login record in the database by `loginID`. This looks up the user trying to log in.
            var login = await _context.Login.FindAsync(loginID);

            // If the login record is not found, the password is empty, or password verification fails, return an error.
            if (login == null || string.IsNullOrEmpty(password) || !s_simpleHash.Verify(password, login.PasswordHash))
            {
                // Add an error message to the ModelState to inform the user that login failed.
                ModelState.AddModelError("LoginFailed", "Login failed, please try again.");

                // Return the login view with the user's `LoginID` pre-filled, allowing them to try again.
                return View(new Login { LoginID = loginID });
            }

            // Retrieve the `CustomerID` associated with the successful login. This will link the login to a customer.
            var custID = login.CustomerID;

            // Fetch the customer details from the database using the `CustomerID`. The `Customer` object is needed for session setup.
            var customer = await _context.Customer.FindAsync(custID);

            // Store the customer's `CustomerID` and `Name` in the session, effectively logging in the customer and tracking their session.
            HttpContext.Session.SetInt32(nameof(Customer.CustomerID), login.CustomerID);
            HttpContext.Session.SetString(nameof(Customer.Name), customer.Name);

            // Redirect the customer to the "Index" action of the `LandingController`. This is typically the main page after login.
            return RedirectToAction("Index", "Landing");
        }

        // Action method to log out the user. The route `/SecureLogin/LogoutNow` is mapped to this action.
        [Route("LogoutNow")]
        public IActionResult Logout()
        {
            // Clears all session data, effectively logging out the user by ending their session.
            HttpContext.Session.Clear();

            // Redirect the user to the home page after logging out. This brings them back to the main "Index" action of the `HomeController`.
            return RedirectToAction("Index", "Home");
        }
    }
}
