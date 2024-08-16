using MCBA_Web_App.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MCBA_Web_App.Controllers
{
    // The HomeController class handles requests for the home-related views in the MCBA web application.
    // It derives from the Controller class, enabling it to manage HTTP requests and return responses.
    public class HomeController : Controller
    {
        // Private field for logging within the controller.
        // ILogger is injected into the controller via Dependency Injection (DI) to log messages or errors.
        private readonly ILogger<HomeController> _logger;

        // Constructor that takes an ILogger instance to allow for logging within the controller.
        // The logger is used to log information and error messages.
        public HomeController(ILogger<HomeController> logger)
        {
            // Assign the injected logger to the private field for use throughout the class.
            _logger = logger;
        }

        // Action method that responds to HTTP GET requests for the home page ("/" or "/Home/Index").
        // It renders the Index view.
        public IActionResult Index()
        {
            // Return the Index view to the user, which will display the home page.
            return View();
        }

        // Action method that responds to HTTP GET requests for the Privacy page ("/Home/Privacy").
        // It renders the Privacy view, typically showing the privacy policy.
        public IActionResult Privacy()
        {
            // Return the Privacy view to the user.
            return View();
        }

        // Action method that handles errors in the application. It is called when an unhandled exception occurs.
        // The [ResponseCache] attribute is used to specify caching behavior.
        // Duration = 0: The response should not be cached.
        // Location = ResponseCacheLocation.None: The cache should not be stored anywhere.
        // NoStore = true: Prevents caching of the response entirely.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // The Error view is returned with an instance of ErrorViewModel.
            // The model includes the RequestId, which is either the current activity's ID or the HTTP context's trace identifier.
            // This helps in tracking and debugging errors by correlating them with a request.
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
