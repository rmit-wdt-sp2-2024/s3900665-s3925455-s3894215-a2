using MCBA_Web_App.Models;
using System.Security.AccessControl;
using System.Security.Principal;
using Xunit;

namespace MCBA_Web_App.Models.Tests
{
    public class AccountTests
    {
        [Fact]
        public void GetMinimumAmountChequeTest()
        {
            // Arrange
            var account = new Account
            {
                AccountNumber = 9999,
                AccountType = 'C',
                CustomerID = 1111,
            };

            // Act
            var result = account.GetMinimumAmount();

            // Assert
            Assert.Equal(300, result);
        }

        [Fact]
        public void GetMinimumAmountSavingsTest()
        {
            // Arrange
            var account = new Account
            {
                AccountNumber = 9999,
                AccountType = 'S',
                CustomerID = 1111,
            };

            // Act
            var result = account.GetMinimumAmount();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void FindBalanceTest()
        {
            var transaction1 = new Transactions
            {
                TransactionID = 996,
                TransactionTimeUtc = DateTime.UtcNow,
                TransactionType = 'D',
                Amount = 10,
                AccountNumber = 9999,
            };

            var transaction2 = new Transactions()
            {
                TransactionID = 998,
                TransactionTimeUtc = DateTime.UtcNow,
                TransactionType = 'D',
                Amount = 55,
                AccountNumber = 9999,
            };

            var transaction3 = new Transactions()
            {
                TransactionID = 999,
                TransactionTimeUtc = DateTime.UtcNow,
                TransactionType = 'W',
                Amount = 26,
                AccountNumber = 9999,
            };

            var account = new Account
            {
                AccountNumber = 9999,
                AccountType = 'C',
                CustomerID = 1111,
                Transactions = new List<Transactions>(),
            };

            account.Transactions.Add(transaction1);
            account.Transactions.Add(transaction2);
            account.Transactions.Add(transaction3);

            var result = account.FindBalance();

            Assert.Equal(39, result);
        }

        [Fact]
        public void ReturnFormattedTypeCheckingTest()
        {
            // Arrange
            var account = new Account
            {
                AccountNumber = 9999,
                AccountType = 'C',
                CustomerID = 1111,
            };

            // Act
            var result = account.ReturnFormattedType();

            // Assert
            Assert.Equal("Checking", result);
        }

        [Fact]
        public void ReturnFormattedTypeSavingsTest()
        {
            // Arrange
            var account = new Account
            {
                AccountNumber = 9999,
                AccountType = 'S',
                CustomerID = 1111,
            };

            // Act
            var result = account.ReturnFormattedType();

            // Assert
            Assert.Equal("Savings", result);
        }


        [Fact]
        public void GetAvailableBalanceTest()
        {
            var transaction1 = new Transactions
            {
                TransactionID = 996,
                TransactionTimeUtc = DateTime.UtcNow,
                TransactionType = 'D',
                Amount = 110.11m,
                AccountNumber = 9999,
            };

            var transaction2 = new Transactions()
            {
                TransactionID = 998,
                TransactionTimeUtc = DateTime.UtcNow,
                TransactionType = 'D',
                Amount = 758.82m,
                AccountNumber = 9999,
            };

            var transaction3 = new Transactions()
            {
                TransactionID = 999,
                TransactionTimeUtc = DateTime.UtcNow,
                TransactionType = 'W',
                Amount = 26.59m,
                AccountNumber = 9999,
            };

            var account = new Account
            {
                AccountNumber = 9999,
                AccountType = 'C',
                CustomerID = 1111,
                Transactions = new List<Transactions>(),
            };

            // 842.34 - min amount 300

            account.Transactions.Add(transaction1);
            account.Transactions.Add(transaction2);
            account.Transactions.Add(transaction3);

            var result = account.GetAvailableBalance();

            Assert.Equal(542.34m, result);
        }
    }
}