using System;  // Provides basic classes and base class libraries for .NET.
using System.Collections.Generic;  // Provides interfaces and classes for collections.
using System.Linq;  // Provides LINQ functionality for querying data.
using System.Threading.Tasks;  // Provides types for asynchronous programming.
using Microsoft.AspNetCore.Mvc;  // Imports ASP.NET Core MVC for creating controllers and handling requests.
using Microsoft.AspNetCore.Mvc.Rendering;  // Provides classes to render HTML elements in Razor views.
using Microsoft.EntityFrameworkCore;  // Provides Entity Framework Core functionalities for database operations.
using MCBA_Web_App.Data;  // Imports the data layer for interacting with the database.
using MCBA_Web_App.Models;  // Imports model classes such as Login and Customer.
using Castle.Core.Resource;  // Provides resource management utilities.
using System.Security.Cryptography;  // Provides cryptographic services like hashing.
using SimpleHashing.Net;  // Imports the SimpleHashing library for securely hashing and verifying passwords.
using ImageMagick;  // Imports ImageMagick for image processing functionalities.
using static System.Runtime.InteropServices.JavaScript.JSType;  // Provides types for JavaScript interop.

namespace MCBA_Web_App.Controllers
{
    // Controller responsible for handling profile-related operations.
    public class ProfileController : Controller
    {
        // Private field for database context to interact with the MCBA database.
        private readonly MCBAContext _context;

        // Static field to hold the hashing implementation. Using `SimpleHash` from SimpleHashing.Net for password verification.
        private static readonly ISimpleHash s_simpleHash = new SimpleHash();

        // Constructor that injects the MCBAContext into the controller. This allows the controller to access the database.
        public ProfileController(MCBAContext context)
        {
            _context = context;
        }

        // Property to retrieve the CustomerID from the session.
        private int CustomerID => HttpContext.Session.GetInt32(nameof(Customer.CustomerID)).Value;

        // GET: Profile
        // Action method to display the profile page for the logged-in customer.
        public async Task<IActionResult> Index()
        {
            // Get the CustomerID from the session.
            int loggedInCustomerId = HttpContext.Session.GetInt32(nameof(Customer.CustomerID)) ?? 0;

            // Retrieve the customer with the logged-in CustomerID.
            var customer = await _context.Customer.FindAsync(loggedInCustomerId);

            if (customer == null)
            {
                // If the customer is not found, return a not found result or handle as needed.
                return NotFound();
            }

            // Pass the customer data to the view.
            return View(customer);
        }

        // GET: Profile/Edit/5
        // Action method to display the edit form for the customer profile.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Customer == null)
            {
                // If the ID is null or the customer context is null, return not found.
                return NotFound();
            }

            // Retrieve the customer by ID.
            var customer = await _context.Customer.FindAsync(id);
            if (customer == null)
            {
                // If the customer is not found, return not found.
                return NotFound();
            }
            return View(customer);
        }

        // POST: Profile/Edit/5
        // Action method to handle the submission of the edited customer profile.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerID,Name,TFN,Address,City,State,Postcode,Mobile,ProfilePictureFileName")] Customer customer)
        {
            if (id != customer.CustomerID)
            {
                // If the ID does not match the customer ID, return not found.
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the customer information in the database.
                    _context.Update(customer);
                    await _context.SaveChangesAsync();

                    // Check if the ProfilePictureFileName property is not null or empty.
                    if (!string.IsNullOrEmpty(customer.ProfilePictureFileName) && Request.Form.Files.Count > 0)
                    {
                        // Call the UploadProfilePicture method to handle the uploaded file.
                        await UploadProfilePicture(Request.Form.Files[0]);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency issues if the customer does not exist.
                    if (!CustomerExists(customer.CustomerID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to the profile index page after a successful update.
                return RedirectToAction(nameof(Index));
            }
            // Return the view with the customer data if the model state is not valid.
            return View(customer);
        }

        // Helper method to check if a customer exists by ID.
        private bool CustomerExists(int id)
        {
            return _context.Customer.Any(e => e.CustomerID == id);
        }

        // Action method to handle the update of the user's password.
        [HttpPost]
        public async Task<IActionResult> UpdatePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {
                // Check if the new password and confirmation password are valid and match.
                if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
                {
                    ModelState.AddModelError("PasswordFailed", "Change Password failed, please try again.");
                    return RedirectToAction("Index"); // Return to the profile view with an error message.
                }

                int loggedInCustomerId = HttpContext.Session.GetInt32(nameof(Customer.CustomerID)) ?? 0;

                // Retrieve the login based on the logged-in customer ID.
                var login = await _context.Login.FirstOrDefaultAsync(l => l.CustomerID == loggedInCustomerId);

                if (login == null)
                {
                    // Handle the case where the login is not found.
                    return RedirectToAction("Index", "Landing");
                }

                // Extract the stored password components (salt and hash).
                var storedHashParts = login.PasswordHash.Split('$');
                var salt = Convert.FromBase64String(storedHashParts[2]);

                // Hash the old password provided by the user.
                byte[] oldPasswordHash;
                using (var deriveBytes = new Rfc2898DeriveBytes(oldPassword, salt, 50000, HashAlgorithmName.SHA1))
                {
                    oldPasswordHash = deriveBytes.GetBytes(32);
                }

                var oldPasswordHashBase64 = Convert.ToBase64String(oldPasswordHash);
                if (storedHashParts[3] != oldPasswordHashBase64)
                {
                    // Old password does not match.
                    ModelState.AddModelError("PasswordFailed", "Old password is incorrect.");
                    return View("Index");
                }

                // Generate a new salt.
                byte[] newSalt;
                using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
                {
                    newSalt = new byte[16];
                    rngCsp.GetBytes(newSalt);
                }

                // Generate a new hash using Rfc2898DeriveBytes.
                byte[] newHash;
                using (var deriveBytes = new Rfc2898DeriveBytes(newPassword, newSalt, 50000, HashAlgorithmName.SHA1))
                {
                    newHash = deriveBytes.GetBytes(32);
                }

                // Convert salt and hash to Base64 strings.
                string saltBase64 = Convert.ToBase64String(newSalt);
                string hashBase64 = Convert.ToBase64String(newHash);

                // Construct the password hash string with the specified format.
                string passwordHashString = $"Rfc2898DeriveBytes$50000${saltBase64}${hashBase64}";

                // Check if the total length is 94 characters.
                if (passwordHashString.Length != 94)
                {
                    // Handle the case where the format is not as expected.
                    ModelState.AddModelError("PasswordFailed", "Invalid password hash format.");
                    return RedirectToAction("Index", "Landing");
                }

                // Update the password hash in the database.
                login.PasswordHash = passwordHashString;

                // Save changes to the database.
                await _context.SaveChangesAsync();

                // Set the success message only if the password update was successful.
                TempData["SuccessMessage"] = "Password updated successfully.";
                return RedirectToAction("Index", "Landing");
            }
            catch (Exception ex)
            {
                // Log or handle the exception.
                Console.WriteLine($"Error updating password: {ex.Message}");
                return RedirectToAction("Index", "Landing");
            }
        }

        // Action method to handle the upload of a profile picture.
        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    // Handle the case where no file is uploaded.
                    return RedirectToAction("Index", "Landing");
                }

                int loggedInCustomerId = HttpContext.Session.GetInt32(nameof(Customer.CustomerID)) ?? 0;
                var customer = await _context.Customer.FindAsync(loggedInCustomerId);

                if (customer == null)
                {
                    // Handle the case where the customer is not found.
                    return RedirectToAction("Index", "Landing");
                }

                using (var imageStream = file.OpenReadStream())
                {
                    // Resize and convert the uploaded image to JPG format.
                    var image = new MagickImage(imageStream);
                    image.Format = MagickFormat.Jpg;
                    image.Resize(new MagickGeometry(400, 400) { IgnoreAspectRatio = false });

                    // Save the image to a storage location.
                    string fileName = $"profile_{Guid.NewGuid()}.jpg";
                    string filePath = Path.Combine("wwwroot", "profile_images", fileName);
                    image.Write(filePath);

                    // Update the Customer model with the file name.
                    customer.ProfilePictureFileName = fileName;
                    await _context.SaveChangesAsync();
                }

                // Redirect to the profile index page after a successful upload.
                return RedirectToAction("Index", "Profile");
            }
            catch (Exception ex)
            {
                // Log or handle the exception.
                Console.WriteLine($"Error uploading profile picture: {ex.Message}");
                return RedirectToAction("Index", "Landing");
            }
        }

        // Action method to delete the user's profile picture.
        [HttpPost]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            try
            {
                int loggedInCustomerId = HttpContext.Session.GetInt32(nameof(Customer.CustomerID)) ?? 0;
                var customer = await _context.Customer.FindAsync(loggedInCustomerId);

                if (customer == null)
                {
                    // Handle the case where the customer is not found.
                    return RedirectToAction("Index", "Landing");
                }

                // Delete the profile picture file from storage.
                string filePath = Path.Combine("wwwroot", "profile_images", customer.ProfilePictureFileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Update the Customer model to remove the profile picture information.
                customer.ProfilePictureFileName = null;
                await _context.SaveChangesAsync();

                // Redirect to the profile index page after successful deletion.
                return RedirectToAction("Index", "Profile");
            }
            catch (Exception ex)
            {
                // Log or handle the exception.
                Console.WriteLine($"Error deleting profile picture: {ex.Message}");
                return RedirectToAction("Index", "Landing");
            }
        }

        // Action method to retrieve the user's profile picture.
        public IActionResult GetProfilePicture()
        {
            try
            {
                int loggedInCustomerId = HttpContext.Session.GetInt32(nameof(Customer.CustomerID)) ?? 0;
                var customer = _context.Customer.Find(loggedInCustomerId);

                if (customer == null || string.IsNullOrEmpty(customer.ProfilePictureFileName))
                {
                    // If the customer or profile picture is not found, return a default image.
                    string defaultImagePath = Path.Combine("~", "profile_images", "default_profile_image.jpg");
                    return PhysicalFile(defaultImagePath, "image/jpeg");
                }

                // Return the customer's profile picture.
                string filePath = Path.Combine("wwwroot", "profile_images", customer.ProfilePictureFileName);
                return PhysicalFile(filePath, "image/jpeg");
            }
            catch (Exception ex)
            {
                // Log or handle the exception.
                Console.WriteLine($"Error getting profile picture: {ex.Message}");
                return RedirectToAction("Index", "Landing");
            }
        }
    }
}
