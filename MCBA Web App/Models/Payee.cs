using System.ComponentModel.DataAnnotations;  // Import for validation attributes such as required fields, string length limits, and regular expressions.

namespace MCBA_Web_App.Models
{
    // The Payee class represents an entity (e.g., a company or individual) to which payments can be made.
    // It stores details like the payee's name, address, city, state, postcode, and phone number.
    public class Payee
    {
        // Primary key for the Payee entity. This uniquely identifies each payee in the system.
        [Key]  // Specifies that PayeeID is the primary key in the database.
        public int PayeeID { get; set; }

        // The name of the payee. It is required and must be a string with a maximum length of 50 characters.
        [Required]  // Ensures that the Name is not null or empty.
        [StringLength(50)]  // Limits the length of the Name to a maximum of 50 characters.
        public string Name { get; set; }

        // The address of the payee. It is required and must be a string with a maximum length of 50 characters.
        // The address must start with an uppercase letter, followed by letters, numbers, spaces, and allowed special characters.
        [Required]  // Ensures that the Address is not null or empty.
        [StringLength(50)]  // Limits the length of the Address to 50 characters.
        [RegularExpression(@"^[A-Z]+[a-zA-Z0-9""'\s-]*$")]  // Validates that the address starts with an uppercase letter and follows the allowed character pattern.
        public string Address { get; set; }

        // The city where the payee is located. It is required and must be a string with a maximum length of 40 characters.
        // The city name must start with an uppercase letter, followed by letters, numbers, spaces, and allowed special characters.
        [Required]  // Ensures that the City is not null or empty.
        [StringLength(40)]  // Limits the length of the City to 40 characters.
        [RegularExpression(@"^[A-Z]+[a-zA-Z0-9""'\s-]*$")]  // Validates that the city starts with an uppercase letter and follows the allowed character pattern.
        public string City { get; set; }

        // The state or region where the payee is located. It is required and must be a string with 2-3 characters.
        // The state name must start with an uppercase letter and follow the allowed character pattern.
        [Required]  // Ensures that the State is not null or empty.
        [StringLength(3, MinimumLength = 2)]  // Limits the length of the State to 2 or 3 characters.
        [RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]  // Validates that the state starts with an uppercase letter and follows the allowed character pattern.
        public string State { get; set; }

        // The postcode of the payee's address. It is required and must be a 4-digit number between 1000 and 9999.
        [Required]  // Ensures that the Postcode is not null or empty.
        [Range(1000, 9999)]  // Validates that the Postcode is a 4-digit number within the specified range.
        public int Postcode { get; set; }

        // The phone number of the payee. It is required and stored as a string to support various phone number formats.
        [Required]  // Ensures that the Phone is not null or empty.
        public string Phone { get; set; }
    }
}
