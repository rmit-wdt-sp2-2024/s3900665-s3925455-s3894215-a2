using System.Net.Http;
using System.Threading.Tasks;

namespace MCBA_Web_App.Data;

class RestAPI
{
    private const string CustomersWebServiceUrl = "https://coreteaching01.csit.rmit.edu.au/~e103884/wdt/services/customers/";

    public static async Task<string> GetCustomerDataAsync()
    {
        using HttpClient client = new();
        return await client.GetStringAsync(CustomersWebServiceUrl);
    }
}
