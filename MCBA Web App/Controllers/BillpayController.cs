﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MCBA_Web_App.Data;
using MCBA_Web_App.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Data.SqlTypes;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MCBA_Web_App.Controllers
{
    public class BillPayController : Controller
    {
        private readonly MCBAContext _context;
        private readonly IServiceProvider _serviceProvider;

        public BillPayController(MCBAContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        // GET: BillPay
        public async Task<IActionResult> Index()
        {
            // Retrieve the logged-in user ID from the session
            int? loggedInUserId = HttpContext.Session.GetInt32(nameof(Customer.CustomerID));

            if (loggedInUserId == null)
            {
                // Handle the case where the user is not logged in
                return RedirectToAction("Index", "Home");
            }

            // Start the BillPayService with CustomerID
            using (var scope = _serviceProvider.CreateScope())
            {
                var billPayService = scope.ServiceProvider.GetRequiredService<BillPayService>();
                billPayService.SetCustomerId(loggedInUserId.Value);
                await billPayService.StartAsync(CancellationToken.None);
            }

            // Fetch bill pays for the logged-in user
            var billPays = await _context.BillPay
                .Include(b => b.Account)
                .Where(b => b.Account.CustomerID == loggedInUserId)
                .ToListAsync();

            return View(billPays);
        }

        // GET: BillPay/Create
        public async Task<IActionResult> Create()
        {
            // Retrieve the logged-in user ID from the session
            int? loggedInUserId = HttpContext.Session.GetInt32(nameof(Customer.CustomerID));

            if (loggedInUserId == null)
            {
                // Handle the case where the user is not logged in
                return RedirectToAction("Index", "Home");
            }

            // Fetch accounts for the logged-in user
            var accounts = await _context.Account
                .Where(a => a.CustomerID == loggedInUserId)
                .ToListAsync();

            ViewBag.Accounts = new SelectList(accounts, "AccountNumber", "AccountNumber");

            return View();
        }

        // POST: BillPay/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BillPayID,AccountNumber,PayeeID,Amount,ScheduleTimeUtc,Period")] BillPay billPay)
        {
            // Validate PayeeID by checking if the Payee exists and is not the same as the AccountNumber
            bool payeeExists = await _context.Account.AnyAsync(a => a.AccountNumber == billPay.PayeeID);
            if (!payeeExists)
            {
                ModelState.AddModelError(nameof(billPay.PayeeID), "The Payee is invalid.");
            }
            else if (billPay.PayeeID == billPay.AccountNumber)
            {
                ModelState.AddModelError(nameof(billPay.PayeeID), "Payee cannot be the same as the account number.");
            }

            // Validate ScheduleTimeUtc to ensure it's not in the past
            if (billPay.ScheduleTimeUtc < DateTime.UtcNow)
            {
                ModelState.AddModelError(nameof(billPay.ScheduleTimeUtc), "Scheduled time cannot be in the past.");
            }

            // Validate ScheduleTimeUtc field to ensure it is within a valid range.
            if (billPay.ScheduleTimeUtc < SqlDateTime.MinValue.Value || billPay.ScheduleTimeUtc > SqlDateTime.MaxValue.Value)
            {
                ModelState.AddModelError("ScheduleTimeUtc", "The scheduled date must be within a valid range.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(billPay);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Re-fetch accounts in case of validation errors.
            int? loggedInUserId = HttpContext.Session.GetInt32(nameof(Customer.CustomerID));
            var accounts = await _context.Account
                .Where(a => a.CustomerID == loggedInUserId)
                .ToListAsync();

            ViewBag.Accounts = new SelectList(accounts, "AccountNumber", "AccountNumber");

            return View(billPay);
        }

        // GET: BillPay/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var billPay = await _context.BillPay.FindAsync(id);
            if (billPay == null)
            {
                return NotFound();
            }

            // Convert UTC date to local time for display in the edit form
            billPay.ScheduleTimeUtc = TimeZoneInfo.ConvertTimeFromUtc(billPay.ScheduleTimeUtc, TimeZoneInfo.Local);

            return View(billPay);
        }

        // POST: BillPay/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BillPayID,AccountNumber,PayeeID,Amount,ScheduleTimeUtc,Period")] BillPay billPay)
        {
            if (id != billPay.BillPayID)
            {
                return NotFound();
            }

            // Validate PayeeID by checking if the Payee exists and is not the same as the AccountNumber
            bool payeeExists = await _context.Account.AnyAsync(a => a.AccountNumber == billPay.PayeeID);
            if (!payeeExists)
            {
                ModelState.AddModelError(nameof(billPay.PayeeID), "The Payee is invalid.");
            }
            else if (billPay.PayeeID == billPay.AccountNumber)
            {
                ModelState.AddModelError(nameof(billPay.PayeeID), "Payee cannot be the same as the account number.");
            }

            // Validate ScheduleTimeUtc to ensure it's not in the past
            if (billPay.ScheduleTimeUtc < DateTime.UtcNow)
            {
                ModelState.AddModelError(nameof(billPay.ScheduleTimeUtc), "Scheduled time cannot be in the past.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Convert user-inputted date to UTC before updating the database
                    billPay.ScheduleTimeUtc = TimeZoneInfo.ConvertTimeToUtc(billPay.ScheduleTimeUtc);

                    _context.Update(billPay);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillPayExists(billPay.BillPayID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(billPay);
        }

        // GET: BillPay/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var billPay = await _context.BillPay
                .Include(b => b.Account)
                .FirstOrDefaultAsync(m => m.BillPayID == id);

            if (billPay == null)
            {
                return NotFound();
            }

            return View(billPay);
        }

        // GET: BillPay/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var billPay = await _context.BillPay
                .Include(b => b.Account)
                .FirstOrDefaultAsync(m => m.BillPayID == id);

            if (billPay == null)
            {
                return NotFound();
            }

            return View(billPay);
        }

        // POST: BillPay/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var billPay = await _context.BillPay.FindAsync(id);
            if (billPay != null)
            {
                _context.BillPay.Remove(billPay);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if a BillPay entry exists
        private bool BillPayExists(int id)
        {
            return _context.BillPay.Any(e => e.BillPayID == id);
        }
    }
}
