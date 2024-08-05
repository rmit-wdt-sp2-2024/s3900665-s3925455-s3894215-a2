using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MCBA_Web_App.Models
{
    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed
    }
    public class BillPay
    {
        [Key]
        public required int BillPayID { get; set; }

        public required int AccountNumber { get; set; }

        public required int PayeeID { get; set; }

        [Column(TypeName = "decimal(18, 2)"), DataType(DataType.Currency)]
        public required decimal Amount { get; set; }

        [Required]
        public DateTime ScheduleTimeUtc { get; set; }

        public required char Period { get; set; }

        public PaymentStatus Status { get; set; }

        [ForeignKey("AccountNumber")]
        public virtual Account Account { get; set; }
    }
}