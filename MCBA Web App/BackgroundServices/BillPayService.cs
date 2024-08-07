using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MCBA_Web_App.Data;
using MCBA_Web_App.Models;
using Microsoft.AspNetCore.Http; // Add this for HttpContext

public class BillPayService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<BillPayService> _logger;
    private int _customerId;

    public BillPayService(IServiceProvider provider, ILogger<BillPayService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public int CustomerId
    {
        get { return _customerId; }
        private set { _customerId = value; }
    }

    public void SetCustomerId(int customerId)
    {
        CustomerId = customerId;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessScheduledPayments(stoppingToken);
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ProcessScheduledPayments(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BillPayService is running...");

        using (var scope = _provider.CreateScope())
        {
            var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            var loggedInUserId = httpContextAccessor.HttpContext?.Session.GetInt32(nameof(Customer.CustomerID));

            _logger.LogInformation($"Logged-in user ID: {loggedInUserId}");

            if (loggedInUserId.HasValue)
            {
                SetCustomerId(loggedInUserId.Value);

                var context = scope.ServiceProvider.GetRequiredService<MCBAContext>();

                // Example: Retrieve the account numbers associated with the logged-in user
                var loggedInUserAccounts = await context.Account
                    .Where(a => a.CustomerID == CustomerId)
                    .Select(a => a.AccountNumber)
                    .ToListAsync();

                if (loggedInUserAccounts.Any())
                {
                    Console.WriteLine($"Processing scheduled payments for user with accounts: {string.Join(", ", loggedInUserAccounts)}");

                    // Get pending and past-due bill payments for the logged-in user
                    var paymentsToProcess = await context.BillPay
                        .Where(b => EF.Property<DateTime>(b, "ScheduleTimeUtc") <= DateTime.UtcNow && b.Account.CustomerID == CustomerId)
                        .ToListAsync();

                    foreach (var payment in paymentsToProcess)
                    {
                        // Retrieve the associated account
                        var account = await context.Account.FindAsync(payment.AccountNumber);

                        // Rest of the logic...
                    }

                    // Save changes to the database
                    await context.SaveChangesAsync();
                }
                else
                {
                    // Handle the case where the user has no associated accounts
                    Console.WriteLine($"User with ID {CustomerId} has no associated accounts.");
                }
            }
            else
            {
                _logger.LogInformation("No logged-in user found. Skipping processing.");
            }
        }
    }
}
