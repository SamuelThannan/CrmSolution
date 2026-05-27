using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using CrmFunctions.Models;

namespace CrmFunctions;

public class CustomerChangedFunction
{
    private readonly ILogger<CustomerChangedFunction> _logger;

    public CustomerChangedFunction(ILogger<CustomerChangedFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(CustomerChangedFunction))]
    public async Task Run(
        [CosmosDBTrigger(
            databaseName: "CrmDatabase",
            containerName: "Customers",
            Connection = "CosmosDbConnection",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)]
        IReadOnlyList<Customer> changedCustomers)
    {
        if (changedCustomers is null || changedCustomers.Count == 0)
            return;

        foreach (var customer in changedCustomers)
        {
            _logger.LogInformation(
                "Kund ändrad: {CustomerId} – {CustomerName}",
                customer.Id, customer.Name);

            if (customer.SalesRepresentative is null ||
                string.IsNullOrWhiteSpace(customer.SalesRepresentative.Email))
            {
                _logger.LogWarning("Kund {CustomerId} saknar säljare med email.", customer.Id);
                continue;
            }

            try
            {
                await SendEmailAsync(customer);
                _logger.LogInformation("Mail skickat till {Email}", customer.SalesRepresentative.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError("Fel vid mailutskick: {Error}", ex.Message);
            }
        }
    }

    private static async Task SendEmailAsync(Customer customer)
    {
        var apiKey = Environment.GetEnvironmentVariable("SendGridApiKey");
        var client = new SendGridClient(apiKey);

        var from = new EmailAddress("s.thannan95@gmail.com", "CRM System");
        var to = new EmailAddress(
            customer.SalesRepresentative.Email,
            customer.SalesRepresentative.Name);

        var subject = $"Du är ansvarig säljare för {customer.Name}";

        var plainText = $"""
            Hej {customer.SalesRepresentative.Name},

            Du har blivit registrerad som ansvarig säljare för följande kund.

            Namn:     {customer.Name}
            Titel:    {customer.Title}
            Telefon:  {customer.Phone}
            Email:    {customer.Email}
            Adress:   {customer.Address}

            Detta är ett automatiskt meddelande från CRM-systemet.
            """;

        var html = $"""
            <h2>Du är ansvarig säljare för {customer.Name}</h2>
            <p>Hej {customer.SalesRepresentative.Name},</p>
            <p>Du har blivit registrerad som ansvarig säljare för följande kund.</p>
            <table>
                <tr><td><b>Namn</b></td><td>{customer.Name}</td></tr>
                <tr><td><b>Titel</b></td><td>{customer.Title}</td></tr>
                <tr><td><b>Telefon</b></td><td>{customer.Phone}</td></tr>
                <tr><td><b>Email</b></td><td>{customer.Email}</td></tr>
                <tr><td><b>Adress</b></td><td>{customer.Address}</td></tr>
            </table>
            <p>Detta är ett automatiskt meddelande från CRM-systemet.</p>
            """;

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, html);
        await client.SendEmailAsync(msg);
    }
}