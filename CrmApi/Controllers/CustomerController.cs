using CrmApi.Models;
using CrmApi.Services;

namespace CrmApi.Controllers;

public static class CustomerController
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        var customers = app.MapGroup("/api/customers").WithTags("Customers");

        customers.MapGet("/", async (CosmosDbService db) =>
        {
            var result = await db.GetAllCustomersAsync();
            return Results.Ok(result);
        });

        customers.MapGet("/{id}", async (string id, CosmosDbService db) =>
        {
            var customer = await db.GetCustomerAsync(id);
            return customer is null ? Results.NotFound() : Results.Ok(customer);
        });

        customers.MapGet("/search/name", async (string q, CosmosDbService db) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.BadRequest("Sökterm (q) krävs.");
            var result = await db.SearchByCustomerNameAsync(q);
            return Results.Ok(result);
        });

        customers.MapGet("/search/salesrep", async (string q, CosmosDbService db) =>
        {
            if (string.IsNullOrWhiteSpace(q))
                return Results.BadRequest("Sökterm (q) krävs.");
            var result = await db.SearchBySalesRepAsync(q);
            return Results.Ok(result);
        });

        customers.MapPost("/", async (CustomerRequest request, CosmosDbService db) =>
        {
            var customer = new Customer
            {
                Name = request.Name,
                Title = request.Title,
                Phone = request.Phone,
                Email = request.Email,
                Address = request.Address,
                SalesRepresentative = new()
                {
                    Name = request.SalesRepresentative.Name,
                    Phone = request.SalesRepresentative.Phone,
                    Email = request.SalesRepresentative.Email
                }
            };

            var created = await db.CreateCustomerAsync(customer);
            return Results.Created($"/api/customers/{created.Id}", created);
        });

        customers.MapPut("/{id}", async (string id, CustomerRequest request, CosmosDbService db) =>
        {
            var existing = await db.GetCustomerAsync(id);
            if (existing is null) return Results.NotFound();

            existing.Name = request.Name;
            existing.Title = request.Title;
            existing.Phone = request.Phone;
            existing.Email = request.Email;
            existing.Address = request.Address;
            existing.SalesRepresentative = new()
            {
                Name = request.SalesRepresentative.Name,
                Phone = request.SalesRepresentative.Phone,
                Email = request.SalesRepresentative.Email
            };

            var updated = await db.UpdateCustomerAsync(id, existing);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        customers.MapDelete("/{id}", async (string id, CosmosDbService db) =>
        {
            var deleted = await db.DeleteCustomerAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}