using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MCBA_Web_App.Models
{
    public class Transactions
    {
        [Key]
        public int TransactionID { get; set; }

        public char TransactionType { get; set; }

        [ForeignKey("Account")]
        public int AccountNumber { get; set; }

        public virtual Account Account { get; set; }
        public int? DestinationAccountNumber { get; set; }
        //public virtual Account DestinationAccount { get; set; }

        [Column(TypeName = "decimal(18, 2)"), DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required, StringLength(30), RegularExpression(@"^[A-Z]+[a-zA-Z0-9""'\s-]*$")]
        public string? Comment { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime TransactionTimeUtc { get; set; }

        public String GetFormattedType()
        {
            String type = "";
            if (TransactionType == 'D')
            {
                type = "Deposit";
            }
            else if (TransactionType == 'W')
            {
                type = "Withraw";
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