using System.ComponentModel.DataAnnotations;

namespace MCBA_Web_App.Models
{
    public class Payee
    {
        [Key]
        public int PayeeID { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [Required, StringLength(50), RegularExpression(@"^[A-Z]+[a-zA-Z0-9""'\s-]*$")]
        public string Address { get; set; }

        [Required, StringLength(40), RegularExpression(@"^[A-Z]+[a-zA-Z0-9""'\s-]*$")]
        public string City { get; set; }

        [Required, StringLength(3, MinimumLength = 2)]
        [RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]
        public string State { get; set; }

        [Required, Range(1000, 9999)]
        public int Postcode { get; set; }

        [Required]
        public string Phone { get; set; }
    }
}