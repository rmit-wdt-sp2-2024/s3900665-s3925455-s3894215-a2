using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA_Web_App.Models
{
    // Enum representing different periods for bill payments.
    public enum BillPayPeriod
    {
        Daily,      // Payment occurs daily.
        Weekly,     // Payment occurs weekly.
        BiWeekly,   // Payment occurs every two weeks.
        Monthly,    // Payment occurs once a month.
        Quarterly,  // Payment occurs every three months.
        Annually    // Payment occurs once a year.
    }

    // Enum representing the status of the bill payment.
    public enum PaymentStatus
    {
        Pending,    // Payment is scheduled but not processed yet.
        Success,    // Payment was processed successfully.
        Failed      // Payment failed during processing.
    }

    // Class representing a bill payment in the system.
    public class BillPay
    {
        // Unique identifier for each BillPay entity, acts as the primary key in the database.
        [Key]
        public int BillPayID { get; set; }

        // The account number from which the bill will be paid.
        [Required]
        public int AccountNumber { get; set; }

        // The ID of the payee to whom the payment will be made.
        [Required]
        public int PayeeID { get; set; }

        // The amount to be paid to the payee.
        // The amount is represented as a decimal with precision to two decimal places.
        // It has a minimum and maximum range enforced.
        [Required]
        [Column(TypeName = "decimal(18, 2)"), DataType(DataType.Currency)]
        [Range(0.01, 10000.00, ErrorMessage = "Amount must be between 0.01 and 10000.00")]
        public decimal Amount { get; set; }

        // The date and time when the payment is scheduled, stored in UTC format.
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ScheduleTimeUtc { get; set; }

        // The period of time indicating how often this bill will be paid.
        [Required]
        public BillPayPeriod Period { get; set; }

        // The status of the payment, indicating if it's pending, successful, or failed.
        public PaymentStatus Status { get; set; }

        // Navigation property to associate this bill payment with an account.
        // The foreign key here is 'AccountNumber' which links to the Account entity.
        [ForeignKey("AccountNumber")]
        public virtual Account Account { get; set; }
    }
}
