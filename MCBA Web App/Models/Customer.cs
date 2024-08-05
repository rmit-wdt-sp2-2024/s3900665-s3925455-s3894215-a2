using System.ComponentModel.DataAnnotations;

namespace MCBA_Web_App.Models
{
    public class Customer
    {
        [Key]
        public required int CustomerID { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]
        public required string Name { get; set; }

        [Range(100000000, 999999999, ErrorMessage = "TFN must be a 9-digit number")]
        public int? TFN { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[A-Za-z0-9]+[A-Za-z0-9""'\s-]*$")]
        public string? Address { get; set; }

        [StringLength(40)]
        [RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]
        public string? City { get; set; }

        [StringLength(100)]
        [RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]
        public string? State { get; set; }

        [Range(0001, 9999, ErrorMessage = "The postcode must be a 4-digit number")]
        public int? Postcode { get; set; }

        [StringLength(12)]
        [RegularExpression(@"^(\+61|0)[ -]?[2-478][0-9]{1,4}[ -]?[0-9]{3,4}[ -]?[0-9]{3,4}$", ErrorMessage = "Invalid phone number format.")]
        public string? Mobile { get; set; }

        public virtual List<Account> Accounts { get; set; }
        public Login Login { get; set; }
    }
}
