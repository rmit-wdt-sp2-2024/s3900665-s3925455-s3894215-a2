using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MCBA_Web_App.Data;
using MCBA_Web_App.Models;
using Microsoft.AspNetCore.Http; // Add this for HttpContext
using Microsoft.Extensions.DependencyInjection;

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
                return RedirectToAction("Login", "Home");
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

            // Provide any necessary data to the view, e.g., Payees, AccountNumbers, etc.
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

            // Provide any necessary data to the view, e.g., Payees, AccountNumbers, etc.
            return View(billPay);
        }

        // Helper method to check if a BillPay entry exists
        private bool BillPayExists(int id)
        {
            return _context.BillPay.Any(e => e.BillPayID == id);
        }
    }
}
