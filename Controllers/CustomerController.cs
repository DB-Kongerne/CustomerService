using Microsoft.AspNetCore.Mvc;
using Models;
using System.Linq;
using System.Diagnostics;

namespace CustomerService.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomerController : ControllerBase
{
    // Listen med kunder
    private static List<Customer> _customers = new List<Customer>()
    {
        new Customer()
        {
            Id = new Guid("c9fcbc4b-d2d1-4664-9079-dae78a1de446"),
            Name = "Æ Fiskehandler",
            Address1 = "Søndergade 3",
            City = "Harboøre",
            PostalCode = 7673,
            ContactName = "Jens Peter Olesen",
            TaxNumber = "133466789"
        },
        new Customer()
        {
            Id = Guid.NewGuid(),
            Name = "Jyske Bank",
            Address1 = "Strandvejen 100",
            City = "Århus",
            PostalCode = 8000,
            ContactName = "Søren Sørensen",
            TaxNumber = "155567890"
        },
        new Customer()
        {
            Id = Guid.NewGuid(),
            Name = "Nordea Bank",
            Address1 = "Enghave Plads 20",
            City = "København",
            PostalCode = 1620,
            ContactName = "Mette Hansen",
            TaxNumber = "123456789"
        }
    };

    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ILogger<CustomerController> logger)
    {
        _logger = logger;
    }

    // Endpoint for at hente en specifik kunde via ID
    [HttpGet("{customerId}", Name = "GetCustomerById")]
    public ActionResult<Customer> GetCustomerById(Guid customerId)
    {
        _logger.LogInformation("Metode GetCustomerById called at {DT}", DateTime.UtcNow.ToLongTimeString());

        var customer = _customers.FirstOrDefault(c => c.Id == customerId);
        if (customer == null)
        {
            return NotFound();
        }
        return Ok(customer);
    }

    // Endpoint for at hente alle kunder
    [HttpGet(Name = "GetAllCustomers")]
    public ActionResult<List<Customer>> GetAllCustomers()
    {
        _logger.LogInformation("Metode GetAllCustomers called at {DT}", DateTime.UtcNow.ToLongTimeString());

        return Ok(_customers);
    }

    // Endpoint for at tilføje en ny kunde via POST
    [HttpPost(Name = "AddCustomer")]
    public ActionResult<Customer> AddCustomer([FromBody] Customer newCustomer)
    {
        _logger.LogInformation("Metode AddCustomer called at {DT}", DateTime.UtcNow.ToLongTimeString());

        // Tjek om ID allerede findes i listen
        if (_customers.Any(c => c.Id == newCustomer.Id))
        {
            // Hvis ID allerede findes, log en advarsel og returner HTTP 409 Conflict
            _logger.LogWarning("Customer with ID {ID} already exists. Conflict at {DT}", newCustomer.Id, DateTime.UtcNow.ToLongTimeString());
            return Conflict(new { message = $"Customer with ID {newCustomer.Id} already exists." });
        }

        // Tildel et nyt ID til kunden, hvis der ikke allerede findes et ID
        newCustomer.Id = Guid.NewGuid();

        // Tilføj den nye kunde til listen
        _customers.Add(newCustomer);

        // Log information om at kunden blev tilføjet
        _logger.LogInformation("New customer with ID {ID} added at {DT}", newCustomer.Id, DateTime.UtcNow.ToLongTimeString());

        // Returnér den oprettede kunde sammen med en 201 Created statuskode
        return CreatedAtRoute("GetCustomerById", new { customerId = newCustomer.Id }, newCustomer);
    }

    [HttpGet("version")]
    public async Task<Dictionary<string, string>> GetVersion()
    {
        var properties = new Dictionary<string, string>();
        var assembly = typeof(Program).Assembly;
        properties.Add("service", "qgt-customer-service");
        var ver = FileVersionInfo.GetVersionInfo(typeof(Program)
        .Assembly.Location).ProductVersion;
        properties.Add("version", ver!);
        try
        {
            var hostName = System.Net.Dns.GetHostName();
            var ips = await System.Net.Dns.GetHostAddressesAsync(hostName);
            var ipa = ips.First().MapToIPv4().ToString();
            properties.Add("hosted-at-address", ipa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            properties.Add("hosted-at-address", "Could not resolve IP-address");
        }
        return properties;
    }
}
