using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA_Web_App.Models
{
    public class Login
    {
        
        [Required]
        [Key]
        public string LoginID { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public int CustomerID { get; set; }

        [Required]
        [StringLength(94)]
        public string PasswordHash { get; set; }

    }
}