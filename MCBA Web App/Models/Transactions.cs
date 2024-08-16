using System.ComponentModel.DataAnnotations.Schema;  // Import for defining database schema attributes such as column types and foreign keys.
using System.ComponentModel.DataAnnotations;  // Import for validation attributes like Required, StringLength, and Key.

namespace MCBA_Web_App.Models
{
    // The Transactions class represents a financial transaction associated with a bank account.
    public class Transactions
    {
        // Primary key for the Transactions table. Each transaction has a unique TransactionID.
        [Key]
        public int TransactionID { get; set; }

        // The type of the transaction, represented by a single character (e.g., 'D' for Deposit, 'W' for Withdrawal).
        public char TransactionType { get; set; }

        // Foreign key linking the transaction to a specific account.
        [ForeignKey("Account")]
        public int AccountNumber { get; set; }

        // Navigation property for the related account.
        public virtual Account Account { get; set; }

        // The account number of the destination account, if this transaction is a transfer.
        public int? DestinationAccountNumber { get; set; }

        // The amount of money involved in the transaction. Stored with a precision of 18 digits and 2 decimal places.
        [Column(TypeName = "decimal(18, 2)"), DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        // A comment or description of the transaction, up to 30 characters long. It must follow a specific format defined by the RegularExpression.
        [Required, StringLength(30), RegularExpression(@"^[A-Z]+[a-zA-Z0-9""'\s-]*$")]
        public string? Comment { get; set; }

        // The date and time when the transaction occurred, stored in UTC format.
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime TransactionTimeUtc { get; set; }

        // Method to return a formatted string describing the transaction type.
        public String GetFormattedType()
        {
            String type = "";

            // Determine the transaction type based on the TransactionType field and set a human-readable description.
            if (TransactionType == 'D')
            {
                type = "Deposit";
            }
            else if (TransactionType == 'W')
            {
                type = "Withdraw";
            }
            else if (TransactionType == 'B')
            {
                type = "BillPay";
            }
            else if (TransactionType == 'S')
            {
                type = "Service Charge";
            }
            else if (TransactionType == 'T')
            {
                if (DestinationAccountNumber == null)
                {
                    type = "Transfer Incoming";
                }
                else
                {
                    type = "Transfer Outgoing";
                }
            }

            return type;
        }
    }
}
