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
            decimal balance = 0;
            if (Transactions == null)
            {
                return 0;
            }
            foreach (var transaction in Transactions)
            {
                if (transaction.TransactionType == 'D')
                {
                    balance += transaction.Amount;
                }
                else if (transaction.TransactionType == 'W' || transaction.TransactionType == 'S' || transaction.TransactionType == 'B')
                {
                    balance -= transaction.Amount;
                }
                else if (transaction.TransactionType == 'T')
                {
                    if (transaction.DestinationAccountNumber == null)
                    {
                        balance += transaction.Amount;
                    }
                    else
                    {
                        balance -= transaction.Amount;
                    }
                }
            }
            return balance;
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
            Transactions.Add(
                new Transactions
                {
                    AccountNumber = acNo,
                    TransactionType = type,
                    Comment = comment,
                    Amount = amount,
                    TransactionTimeUtc = DateTime.UtcNow
                });
        }

        public void AddTransfer(int sourceAccount, int destinationAccountNumber, decimal amount, String comment)
        {
            var transferTimeUtc = DateTime.UtcNow;
            Transactions.Add(
                new Transactions //Transfering From
                {
                    AccountNumber = sourceAccount,
                    DestinationAccountNumber = destinationAccountNumber,
                    TransactionType = 'T',
                    Comment = comment,
                    Amount = amount,
                    TransactionTimeUtc = transferTimeUtc
                });
        }

        public decimal GetAvailableBalance()
        {
            decimal availableBalance = FindBalance() - GetMinimumAmount();            
            return availableBalance;
        }
    }
}
