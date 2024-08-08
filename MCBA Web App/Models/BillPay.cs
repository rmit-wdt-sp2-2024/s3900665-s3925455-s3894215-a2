using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA_Web_App.Models
{
    public enum BillPayPeriod
    {
        Daily,
        Weekly,
        BiWeekly,
        Monthly,
        Quarterly,
        Annually
    }

    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed
    }

    public class BillPay
    {
        [Key]
        public int BillPayID { get; set; }

        [Required]
        public int AccountNumber { get; set; }

        [Required]
        public int PayeeID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)"), DataType(DataType.Currency)]
        [Range(0.01, 10000.00, ErrorMessage = "Amount must be between 0.01 and 10000.00")]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ScheduleTimeUtc { get; set; }

        [Required]
        public BillPayPeriod Period { get; set; }

        public PaymentStatus Status { get; set; }

        [ForeignKey("AccountNumber")]
        public virtual Account Account { get; set; }
    }
}
