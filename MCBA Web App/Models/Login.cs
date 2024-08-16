using System.ComponentModel.DataAnnotations;  // Import for data annotations used for validation and model configuration.
using System.ComponentModel.DataAnnotations.Schema;  // Import for defining foreign key relationships in models.

namespace MCBA_Web_App.Models
{
    // The Login class represents the login credentials for a customer in the system.
    // It stores the login ID, associated customer ID, and the hashed password.
    public class Login
    {
        // The LoginID serves as the primary key for the Login entity.
        // It is required and must be unique for each customer in the system.
        [Required]  // Ensures that LoginID is not null.
        [Key]  // Marks this property as the primary key in the database.
        public string LoginID { get; set; }

        // Foreign key linking the login information to the Customer entity.
        // It is required and establishes a relationship between a login and a specific customer.
        [Required]  // Ensures that CustomerID is not null.
        [ForeignKey("Customer")]  // Defines a foreign key relationship with the Customer entity.
        public int CustomerID { get; set; }

        // Stores the hashed version of the customer's password.
        // Password hashing ensures secure storage of passwords instead of plain text.
        // The string length is limited to 94 characters, which is sufficient for typical hash lengths (e.g., SHA-256 or bcrypt).
        [Required]  // Ensures that the PasswordHash is not null.
        [StringLength(94)]  // Restricts the maximum length of the password hash to 94 characters.
        public string PasswordHash { get; set; }

    }
}
