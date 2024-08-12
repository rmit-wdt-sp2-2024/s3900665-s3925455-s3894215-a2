using MCBA_Web_App.Models;
using System.Security.AccessControl;
using System.Security.Principal;
using Xunit;

namespace MCBA_Web_App.Tests
{
    public class CustomerTests
    { 
        [Fact]
        public void CheckForFreeTransactionsTest()
        {
            // Arrange
            var customer = new Customer
            {
                CustomerID = 1111,
                Name = "Test",
                FreeTransactions = 5,
            };

            // Act
            var result = customer.CheckForFreeTransactions();

            // Assert
            Assert.True(result);
        }
    }
}
