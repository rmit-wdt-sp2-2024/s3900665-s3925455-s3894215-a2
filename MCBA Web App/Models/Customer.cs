using System.ComponentModel.DataAnnotations;

namespace MCBA_Web_App.Models
{
    public class Customer
    {
        [Key]
        public required int CustomerID { get; set; }

        public required string Name { get; set; }

        public int? TFN { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public int? Postcode { get; set; }

        public string? Mobile { get; set; }

        public virtual List<Account> Accounts { get; set; }
        public Login Login { get; set; }
    }
}
