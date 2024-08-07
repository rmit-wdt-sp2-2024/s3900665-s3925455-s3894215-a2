using Castle.Core.Resource;
using MCBA_Web_App.Authorise;
using MCBA_Web_App.Data;
using MCBA_Web_App.Models;
using MCBA_Web_App.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MCBA_Web_App.Controllers
{
    [AuthorizeCustomer]
    public class LandingController : Controller
    {
        private readonly MCBAContext _context;
        public LandingController(MCBAContext context) => _context = context;
        private int CustomerID => HttpContext.Session.GetInt32(nameof(Customer.CustomerID)).Value;
        public async Task<IActionResult> Index()
        {
            var customer = _context.Customer
                .Include(c => c.Accounts)
                .FirstOrDefault(c => c.CustomerID == CustomerID);

            if (customer != null)
            {
                // Load transactions for all accounts associated with the customer
                var accountNumbers = customer.Accounts.Select(acc => acc.AccountNumber).ToList();

                var transactions = _context.Transaction
                    .Where(t => accountNumbers.Contains(t.AccountNumber))
                    .ToList();

                // Assign transactions to the respective accounts
                foreach (var account in customer.Accounts)
                {
                    account.Transactions = transactions.Where(t => t.AccountNumber == account.AccountNumber).ToList();
                }
            }

            //var customer = await LoadCustomerWithTransactions();

            return View(customer);
        }

        public async Task<IActionResult> Deposit(int id) => View(await _context.Account.FindAsync(id));

        [HttpPost]
        public async Task<IActionResult> Deposit(int id, decimal amount, string comment)
        {
            var account = await _context.Account
                .Include(x => x.Transactions)
                .Where(account => account.Transactions.Any(transaction => transaction.AccountNumber == id))
                .FirstOrDefaultAsync();

            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            else if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");

            if (!ModelState.IsValid)
            {
                ViewBag.Comment = comment;
                ViewBag.Amount = amount;
                return View(account);
            }

            account.AddTransaction(id, amount, comment, 'D');

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Withdraw(int id) => View(await _context.Account
                .Include(x => x.Transactions)
                .Where(account => account.Transactions.Any(transaction => transaction.AccountNumber == id))
                .FirstOrDefaultAsync());

        [HttpPost]
        public async Task<IActionResult> Withdraw(int id, decimal amount, string comment)
        {
            var account = await _context.Account
                .Include(x => x.Transactions)
                .Where(account => account.Transactions.Any(transaction => transaction.AccountNumber == id))
                .FirstOrDefaultAsync();

            var myCustomer = await _context.Customer
                .FirstOrDefaultAsync(c => c.CustomerID == CustomerID);

            bool freeTransactions = myCustomer.CheckForFreeTransactions();
            decimal availableBalance = account.GetAvailableBalance();
            decimal fees = 0.05m;

            if (!freeTransactions)
            {
                availableBalance = availableBalance - fees;
            }

            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            else if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
            else if (availableBalance < amount)
                ModelState.AddModelError(nameof(comment), "Error! Insufficent funds");

            if (!ModelState.IsValid)
            {
                ViewBag.Comment = comment;
                ViewBag.Amount = amount;
                return View(account);
            }

            account.AddTransaction(id, amount, comment, 'W');

            if (freeTransactions)
            {
                myCustomer.FreeTransactions--;
            }
            else
            {
                account.AddTransaction(id, fees, "Service Charge", 'S');
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Transfer(int id) => View(await _context.Account
                .Include(x => x.Transactions)
                .Where(account => account.Transactions.Any(transaction => transaction.AccountNumber == id))
                .FirstOrDefaultAsync());

        [HttpPost]
        public async Task<IActionResult> Transfer(int id, int destAccount, decimal amount, string comment)
        {
            var account = await _context.Account
                .Include(x => x.Transactions)
                .Where(account => account.Transactions.Any(transaction => transaction.AccountNumber == id))
                .FirstOrDefaultAsync();

            var accountNumbers = await _context.Transaction
                .Select(x => x.AccountNumber)
                .Distinct()
                .ToListAsync();

            var myCustomer = await _context.Customer
                .FirstOrDefaultAsync(c => c.CustomerID == CustomerID);

            bool findAccount = false;
            for (int i = 0; i < accountNumbers.Count; i++)
            {
                if (destAccount != id && accountNumbers[i] == destAccount)
                {
                    findAccount = true;
                    break;
                }
            }

            bool freeTransactions = myCustomer.CheckForFreeTransactions();
            decimal availableBalance = account.GetAvailableBalance();
            decimal fees = 0.10m;

            if (!freeTransactions)
            {
                availableBalance -= fees;
            }

            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            else if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
            else if (availableBalance < amount)
                ModelState.AddModelError(nameof(comment), "Error! Insufficent funds");
            else if (!findAccount)
                ModelState.AddModelError(nameof(destAccount), "The Destination account is invalid");

            if (!ModelState.IsValid)
            {
                ViewBag.Comment = comment;
                ViewBag.Amount = amount;
                ViewBag.DestAccount = destAccount;
                return View(account);
            }

            account.AddTransfer(id, destAccount, amount, comment);

            var destinationAccount = await _context.Account
                .Include(x => x.Transactions)
                .Where(account => account.Transactions.Any(transaction => transaction.AccountNumber == destAccount))
                .FirstOrDefaultAsync();

            if (freeTransactions)
            {
                myCustomer.FreeTransactions--;
            }
            else
            {
                account.AddTransaction(id, fees, "Service Charge", 'S');
            }

            destinationAccount.AddTransaction(destAccount, amount, comment, 'T');

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Statements(int id, int page = 1)
        {
            int pageSize = 4;

            var transactions = _context.Transaction
                .Where(t => t.AccountNumber == id)
                .OrderByDescending(t => t.TransactionTimeUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.AccountId = id;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(_context.Transaction.Count(t => t.AccountNumber == id) / (double)pageSize);

            return View(transactions);
        }

        private async Task<Customer> LoadCustomerWithTransactions()
        {
            var customer = await _context.Customer
            .Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.CustomerID == CustomerID);

            if (customer != null)
            {
                var accountNumbers = customer.Accounts.Select(acc => acc.AccountNumber).ToList();

                var transactions = _context.Transaction
                    .Where(t => accountNumbers.Contains(t.AccountNumber))
                    .ToList();

                foreach (var account in customer.Accounts)
                {
                    account.Transactions = transactions.Where(t => t.AccountNumber == account.AccountNumber).ToList();
                }
            }

            return customer;
        }
    }
}