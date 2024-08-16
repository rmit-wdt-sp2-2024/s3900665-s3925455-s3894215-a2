using System.ComponentModel.DataAnnotations;  // Import for validation attributes, such as data annotations for validation rules.

namespace MCBA_Web_App.Models
{
    // The Customer class represents a customer within the system, containing details such as their name, address, and related accounts.
    public class Customer
    {
        // Primary key for the Customer entity, uniquely identifies each customer in the database.
        [Key]
        public required int CustomerID { get; set; }

        // The name of the customer. It is required, must be a string of up to 50 characters, 
        // and must start with an uppercase letter and contain only letters, spaces, and allowed characters.
        [StringLength(50)]
        [RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]
        public required string Name { get; set; }

        // Tax File Number (TFN) of the customer. It is optional, but if provided, it must be a 9-digit number.
        [Range(100000000, 999999999, ErrorMessage = "TFN must be a 9-digit number")]
        public int? TFN { get; set; }

        // The address of the customer. It is optional and must be a string of up to 50 characters,
        // starting with alphanumeric characters and allowing certain special characters.
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z0-9]+[A-Za-z0-9""'\s-]*$")]
        public string? Address { get; set; }

        // The city where the customer resides. It is optional, must be a string of up to 40 characters,
        // and must start with an uppercase letter, followed by letters, spaces, and allowed characters.
        [StringLength(40)]
        [RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]
        public string? City { get; set; }

        // The state of residence for the customer. It is optional, must be a string of up to 100 characters,
        // and must start with an uppercase letter and only contain letters, spaces, and allowed characters.
        [StringLength(100)]
        [RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]
        public string? State { get; set; }

        // The postcode of the customer’s address. It is optional but must be a 4-digit number if provided.
        [Range(0001, 9999, ErrorMessage = "The postcode must be a 4-digit number")]
        public int? Postcode { get; set; }

        // The mobile phone number of the customer. It is optional and must adhere to a specific phone number format,
        // allowing Australian phone numbers with the country code or standard formatting.
        [StringLength(12)]
        [RegularExpression(@"^(\+61|0)[ -]?[2-478][0-9]{1,4}[ -]?[0-9]{3,4}[ -]?[0-9]{3,4}$", ErrorMessage = "Invalid phone number format.")]
        public string? Mobile { get; set; }

        // Navigation property representing a list of accounts associated with the customer. 
        // A customer can have multiple bank accounts.
        public virtual List<Account> Accounts { get; set; }

        // Navigation property representing the login credentials associated with the customer.
        // Each customer has one Login object.
        public Login Login { get; set; }

        // The file name of the profile picture, stored as a string. It is optional.
        public string? ProfilePictureFileName { get; set; }

        // Stores the number of free transactions the customer has available. It is optional.
        public int? FreeTransactions { get; set; }

        // Method to check if the customer has any free transactions available.
        // Returns false if FreeTransactions is 0 or null (indicating no free transactions).
        public bool CheckForFreeTransactions()
        {
            if (FreeTransactions == 0 || FreeTransactions == null)
            {
                return false;  // No free transactions available.
            }
            return true;  // Free transactions are available.
        }
    }
}
