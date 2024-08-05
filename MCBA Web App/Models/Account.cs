using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA_Web_App.Models
{
    public class Account
    {
        [Key]
        public required int AccountNumber { get; set; }

        public required char AccountType { get; set; }

        [ForeignKey("Customer")]
        public required int CustomerID { get; set; }

        public virtual Customer Customer { get; set; }

        //Used for Seeding data
        [InverseProperty("Account")]
        public virtual List<Transactions> Transactions { get; set; }

        public decimal FindBalance()
        {
            return 0;
        }

        public decimal GetMinimumAmount() {
            decimal amount = 0;
            if (AccountType=='C')
            {
                amount = 300;
            }
            return amount;
        }

        public String ReturnFormattedType() { 
            String savingsOrChecking="";
            if (AccountType == 'S') {
                savingsOrChecking = "Savings";
                    } else {
                savingsOrChecking = "Checking";
            }
            return savingsOrChecking;
        }

        public void AddTransaction(int acNo, decimal amount, String comment, char type)
        {
        }

        public void AddTransfer(int sourceAccount, int destinationAccountNumber, decimal amount, String comment)
        {
        }

        public decimal GetAvailableBalance()
        {
            decimal availableBalance = FindBalance() - GetMinimumAmount();            
            return availableBalance;
        }
    }
}
