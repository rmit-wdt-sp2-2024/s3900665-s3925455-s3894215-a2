using Microsoft.EntityFrameworkCore;  // Imports Entity Framework Core for database interactions.
using System.ComponentModel.DataAnnotations;  // Provides data annotations for model properties.
using System.ComponentModel.DataAnnotations.Schema;  // Enables defining foreign key relationships and database mappings.

namespace MCBA_Web_App.Models
{
    // The Account class represents a bank account within the system, containing details about the account number, type, associated customer, and transactions.
    public class Account
    {
        // Primary key for the account entity. 'AccountNumber' uniquely identifies each account in the database.
        [Key]
        public required int AccountNumber { get; set; }

        // Type of account (e.g., 'S' for Savings, 'C' for Checking). It is a required property.
        public required char AccountType { get; set; }

        // Foreign key to the Customer entity, linking this account to a specific customer.
        [ForeignKey("Customer")]
        public required int CustomerID { get; set; }

        // Navigation property representing the customer that owns the account. The virtual keyword enables lazy loading of related data.
        public virtual Customer Customer { get; set; }

        // A collection of transactions related to this account. Each account can have multiple transactions.
        // InverseProperty is used to define the relationship between Account and Transactions.
        [InverseProperty("Account")]
        public virtual List<Transactions> Transactions { get; set; }

        // Method to calculate the current balance of the account based on its transactions.
        public decimal FindBalance()
        {
            decimal balance = 0;

            // If the account has no transactions, the balance is 0.
            if (Transactions == null)
            {
                return 0;
            }

            // Loop through each transaction and adjust the balance based on the type of transaction.
            foreach (var transaction in Transactions)
            {
                // If the transaction type is 'D' (Deposit), add the amount to the balance.
                if (transaction.TransactionType == 'D')
                {
                    balance += transaction.Amount;
                }
                // If the transaction type is 'W' (Withdrawal), 'S' (Service charge), or 'B' (Bill pay), subtract the amount.
                else if (transaction.TransactionType == 'W' || transaction.TransactionType == 'S' || transaction.TransactionType == 'B')
                {
                    balance -= transaction.Amount;
                }
                // If the transaction type is 'T' (Transfer), handle both incoming and outgoing transfers.
                else if (transaction.TransactionType == 'T')
                {
                    // If the DestinationAccountNumber is null, it means the transfer is incoming, so add the amount.
                    if (transaction.DestinationAccountNumber == null)
                    {
                        balance += transaction.Amount;
                    }
                    // Otherwise, it is an outgoing transfer, so subtract the amount.
                    else
                    {
                        balance -= transaction.Amount;
                    }
                }
            }

            // Return the calculated balance.
            return balance;
        }

        // Method to determine the minimum required balance for the account, based on the account type.
        public decimal GetMinimumAmount()
        {
            decimal amount = 0;

            // If the account type is 'C' (Checking), the minimum required balance is $300.
            if (AccountType == 'C')
            {
                amount = 300;
            }

            // Return the minimum balance.
            return amount;
        }

        // Method to return a formatted string representing the type of the account (e.g., "Savings" or "Checking").
        public String ReturnFormattedType()
        {
            String savingsOrChecking = "";

            // If the account type is 'S', return "Savings".
            if (AccountType == 'S')
            {
                savingsOrChecking = "Savings";
            }
            // Otherwise, return "Checking".
            else
            {
                savingsOrChecking = "Checking";
            }

            // Return the formatted account type.
            return savingsOrChecking;
        }

        // Method to add a new transaction (deposit or withdrawal) to the account's transaction list.
        public void AddTransaction(int acNo, decimal amount, String comment, char type)
        {
            // Create a new transaction and add it to the list of transactions.
            Transactions.Add(
                new Transactions
                {
                    AccountNumber = acNo,  // Set the account number for the transaction.
                    TransactionType = type,  // Set the type of transaction ('D' for Deposit, 'W' for Withdrawal, etc.).
                    Comment = comment,  // Include a comment describing the transaction.
                    Amount = amount,  // Set the transaction amount.
                    TransactionTimeUtc = DateTime.UtcNow  // Set the transaction time to the current UTC time.
                });
        }

        // Method to add a transfer transaction from one account to another.
        public void AddTransfer(int sourceAccount, int destinationAccountNumber, decimal amount, String comment)
        {
            var transferTimeUtc = DateTime.UtcNow;  // Set the current UTC time for the transfer.

            // Create a new transfer transaction and add it to the list of transactions.
            Transactions.Add(
                new Transactions
                {
                    AccountNumber = sourceAccount,  // Set the source account number for the transfer.
                    DestinationAccountNumber = destinationAccountNumber,  // Set the destination account number.
                    TransactionType = 'T',  // Mark the transaction as a transfer ('T').
                    Comment = comment,  // Include a comment describing the transfer.
                    Amount = amount,  // Set the transfer amount.
                    TransactionTimeUtc = transferTimeUtc  // Record the transfer time.
                });
        }

        // Method to calculate the available balance by subtracting the minimum required amount from the current balance.
        public decimal GetAvailableBalance()
        {
            // Calculate the available balance as the total balance minus the minimum required balance.
            decimal availableBalance = FindBalance() - GetMinimumAmount();

            // Return the available balance.
            return availableBalance;
        }
    }
}
