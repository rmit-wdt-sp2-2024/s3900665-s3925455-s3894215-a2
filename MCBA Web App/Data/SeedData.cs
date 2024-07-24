using MCBA_Web_App.Models;
using Newtonsoft.Json;


namespace MCBA_Web_App.Data;

public class SeedData
{
    private readonly RestAPI RestApi = new();
    //private readonly DbContextOptions<BankingContext> options;

    public static async Task LoadDataAsync(MCBAContext context)
    {
        // Look for any customers.
        if (context.Customer.Any())
        {
            return;   // DB has been seeded
        }
        //Make a REST call to the customers web-service
        string json = await RestAPI.GetCustomerDataAsync();

        //Deserialize JSON and insert data into the database
        //List<Customer> customers = JsonConvert.DeserializeObject<List<Customer>>(json);
        var customers = JsonConvert.DeserializeObject<List<Customer>>(json);

        //Process and insert data into the database
        Console.WriteLine(json + "\n");
        foreach (var customer in customers)
        {
            customer.FreeTransactions = 2;
            context.Customer.Add(customer);

            foreach (var account in customer.Accounts)
            {
                context.Account.Add(account);

                foreach (var transaction in account.Transactions)
                {
                    context.Transaction.Add(transaction);
                }
            }
            context.Login.Add(customer.Login);
        }
   
        // Save changes to the database
        context.SaveChanges();

        Console.WriteLine("Data loaded successfully.\n");
    }
}