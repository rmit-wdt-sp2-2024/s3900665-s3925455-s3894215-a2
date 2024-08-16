using Castle.Core.Resource;  // Import Castle.Core library for managing resources.
using MCBA_Web_App.Authorise;  // Import custom authorization class specific to the web app.
using MCBA_Web_App.Data;  // Import the data access layer (for database context).
using MCBA_Web_App.Models;  // Import the model classes (Customer, Account, etc.).
using MCBA_Web_App.Utilities;  // Import custom utility classes or extensions.
using Microsoft.AspNetCore.Mvc;  // Import ASP.NET Core MVC framework.
using Microsoft.EntityFrameworkCore;  // Import Entity Framework Core for data querying.

namespace MCBA_Web_App.Controllers
{
    // Apply authorization to ensure only customers can access the controller's actions.
    [AuthorizeCustomer]
    public class LandingController : Controller
    {
        // Database context to interact with the MCBA database.
        private readonly MCBAContext _context;

        // Constructor to inject the MCBAContext dependency.
        public LandingController(MCBAContext context) => _context = context;

        // Property to retrieve the currently logged-in customer's ID from session data.
        private int CustomerID => HttpContext.Session.GetInt32(nameof(Customer.CustomerID)).Value;

        // Action to display the main landing page with customer information.
        public async Task<IActionResult> Index()
        {
            // Load the customer and their associated accounts from the database based on CustomerID.
            var customer = _context.Customer
                .Include(c => c.Accounts)  // Include associated accounts.
                .FirstOrDefault(c => c.CustomerID == CustomerID);  // Find the customer by ID.

            if (customer != null)
            {
                // Retrieve account numbers of all associated accounts.
                var accountNumbers = customer.Accounts.Select(acc => acc.AccountNumber).ToList();

                // Load transactions for all customer accounts from the database.
                var transactions = _context.Transaction
                    .Where(t => accountNumbers.Contains(t.AccountNumber))
                    .ToList();

                // Assign the loaded transactions to the respective accounts.
                foreach (var account in customer.Accounts)
                {
                    account.Transactions = transactions.Where(t => t.AccountNumber == account.AccountNumber).ToList();
                }
            }

            // Return the customer object to the view to display.
            return View(customer);
        }

        // Action to display the deposit view for a specific account.
        public async Task<IActionResult> Deposit(int id) => View(await _context.Account.FindAsync(id));

        // Post action to handle deposit transactions.
        [HttpPost]
        public async Task<IActionResult> Deposit(int id, decimal amount, string comment)
        {
            // Find the account with its transactions.
            var account = await _context.Account
                .Include(x => x.Transactions)
                .Where(account => account.Transactions.Any(transaction => transaction.AccountNumber == id))
                .FirstOrDefaultAsync();

            // Validate the deposit amount.
            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            else if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");

            // If the model is not valid, return the current view with the entered values.
            if (!ModelState.IsValid)
            {
                ViewBag.Comment = comment;
                ViewBag.Amount = amount;
                return View(account);
            }

            // Add the deposit transaction to the account.
            account.AddTransaction(id, amount, comment, 'D');

            // Save changes to the database.
            await _context.SaveChangesAsync();

            // Redirect to the index page.
            return RedirectToAction(nameof(Index));
        }

        // Action to display the withdrawal view for a specific account.
        public async Task<IActionResult> Withdraw(int id) => View(await _context.Account
                .Include(x => x.Transactions)
                .Where(account => account.Transactions.Any(transaction => transaction.AccountNumber == id))
                .FirstOrDefaultAsync());

        // Post action to handle withdrawal transactions.
        [HttpPost]
        public async Task<IActionResult> Withdraw(int id, decimal amount, string comment)
        {
            // Find the account with its transactions.
            var account = await _context.Account
                .Include(x => x.Transactions)
                .Where(account => account.Transactions.Any(transaction => transaction.AccountNumber == id))
                .FirstOrDefaultAsync();

            // Get the current customer.
            var myCustomer = await _context.Customer
                .FirstOrDefaultAsync(c => c.CustomerID == CustomerID);

            // Check if the customer has free transactions.
            bool freeTransactions = myCustomer.CheckForFreeTransactions();
            decimal availableBalance = account.GetAvailableBalance();
            decimal fees = 0.05m;

            // Deduct fees if no free transactions are available.
            if (!freeTransactions)
            {
                availableBalance = availableBalance - fees;
            }

            // Validate the withdrawal amount.
            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            else if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
            else if (availableBalance < amount)
                ModelState.AddModelError(nameof(comment), "Error! Insufficient funds");

            // If the model is not valid, return the current view with the entered values.
            if (!ModelState.IsValid)
            {
                ViewBag.Comment = comment;
                ViewBag.Amount = amount;
                return View(account);
            }

            // Add the withdrawal transaction to the account.
            account.AddTransaction(id, amount, comment, 'W');

            // Deduct a free transaction or add a service charge.
            if (freeTransactions)
            {
                myCustomer.FreeTransactions--;
            }
            else
            {
                account.AddTransaction(id, fees, "Service Charge", 'S');
            }

            // Save changes to the database.
            await _context.SaveChangesAsync();

            // Redirect to the index page.
            return RedirectToAction(nameof(Index));
        }

        // Action to display the transfer view for a specific account.
        public async Task<IActionResult> Transfer(int id) => View(await _context.Account
                .Include(x => x.Transactions)
                .Where(account => account.Transactions.Any(transaction => transaction.AccountNumber == id))
                .FirstOrDefaultAsync());

        // Post action to handle transfer transactions.
        [HttpPost]
        public async Task<IActionResult> Transfer(int id, int destAccount, decimal amount, string comment)
        {
            // Find the account with its transactions.
            var account = await _context.Account
                .Include(x => x.Transactions)
                .Where(account => account.Transactions.Any(transaction => transaction.AccountNumber == id))
                .FirstOrDefaultAsync();

            // Retrieve all distinct account numbers for validation.
            var accountNumbers = await _context.Transaction
                .Select(x => x.AccountNumber)
                .Distinct()
                .ToListAsync();

            // Get the current customer.
            var myCustomer = await _context.Customer
                .FirstOrDefaultAsync(c => c.CustomerID == CustomerID);

            // Check if the destination account exists and is not the same as the source account.
            bool findAccount = false;
            for (int i = 0; i < accountNumbers.Count; i++)
            {
                if (destAccount != id && accountNumbers[i] == destAccount)
                {
                    findAccount = true;
                    break;
                }
            }

            // Check if the customer has free transactions.
            bool freeTransactions = myCustomer.CheckForFreeTransactions();
            decimal availableBalance = account.GetAvailableBalance();
            decimal fees = 0.10m;

            // Deduct fees if no free transactions are available.
            if (!freeTransactions)
            {
                availableBalance -= fees;
            }

            // Validate the transfer amount.
            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            else if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
            else if (availableBalance < amount)
                ModelState.AddModelError(nameof(comment), "Error! Insufficient funds");
            else if (!findAccount)
                ModelState.AddModelError(nameof(destAccount), "The Destination account is invalid");

            // If the model is not valid, return the current view with the entered values.
            if (!ModelState.IsValid)
            {
                ViewBag.Comment = comment;
                ViewBag.Amount = amount;
                ViewBag.DestAccount = destAccount;
                return View(account);
            }

            // Add the transfer transaction to the source account.
            account.AddTransfer(id, destAccount, amount, comment);

            // Find the destination account.
            var destinationAccount = await _context.Account
                .Include(x => x.Transactions)
                .Where(account => account.Transactions.Any(transaction => transaction.AccountNumber == destAccount))
                .FirstOrDefaultAsync();

            // Deduct a free transaction or add a service charge.
            if (freeTransactions)
            {
                myCustomer.FreeTransactions--;
            }
            else
            {
                account.AddTransaction(id, fees, "Service Charge", 'S');
            }

            // Add the transfer transaction to the destination account.
            destinationAccount.AddTransaction(destAccount, amount, comment, 'T');

            // Save changes to the database.
            await _context.SaveChangesAsync();

            // Redirect to the index page.
            return RedirectToAction(nameof(Index));
        }

        // Action to display the transaction statements for an account.
        public async Task<IActionResult> Statements(int id, int page = 1)
        {
            int pageSize = 4;  // Number of transactions per page.

            // Query transactions for the specific account, paginated and ordered by most recent.
            var transactions = _context.Transaction
                .Where(t => t.AccountNumber == id)
                .OrderByDescending(t => t.TransactionTimeUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Provide pagination data to the view.
            ViewBag.AccountId = id;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(_context.Transaction.Count(t => t.AccountNumber == id) / (double)pageSize);

            // Return the transactions to the view.
            return View(transactions);
        }

        // Helper method to load customer with all their transactions.
        private async Task<Customer> LoadCustomerWithTransactions()
        {
            // Load the customer and associated accounts.
            var customer = await _context.Customer
                .Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.CustomerID == CustomerID);

            if (customer != null)
            {
                // Retrieve account numbers of all associated accounts.
                var accountNumbers = customer.Accounts.Select(acc => acc.AccountNumber).ToList();

                // Load transactions for all accounts.
                var transactions = _context.Transaction
                    .Where(t => accountNumbers.Contains(t.AccountNumber))
                    .ToList();

                // Assign transactions to each account.
                foreach (var account in customer.Accounts)
                {
                    account.Transactions = transactions.Where(t => t.AccountNumber == account.AccountNumber).ToList();
                }
            }

            return customer;
        }
    }
}
