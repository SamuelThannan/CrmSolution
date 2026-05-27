using Microsoft.Azure.Cosmos;
using CrmApi.Models;

namespace CrmApi.Services;

public class CosmosDbService
{
    private readonly Container _container;

    public CosmosDbService(CosmosClient client, IConfiguration config)
    {
        var databaseName = config["CosmosDb:DatabaseName"]!;
        var containerName = config["CosmosDb:ContainerName"]!;
        _container = client.GetContainer(databaseName, containerName);
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        var response = await _container.CreateItemAsync(
            customer,
            new PartitionKey(customer.PartitionKey));
        return response.Resource;
    }

    public async Task<Customer?> GetCustomerAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Customer>(
                id,
                new PartitionKey("customer"));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.partitionKey = 'customer'");
        return await RunQueryAsync(query);
    }

    public async Task<IEnumerable<Customer>> SearchByCustomerNameAsync(string name)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.partitionKey = 'customer' " +
            "AND CONTAINS(LOWER(c.name), LOWER(@name))")
            .WithParameter("@name", name);
        return await RunQueryAsync(query);
    }

    public async Task<IEnumerable<Customer>> SearchBySalesRepAsync(string salesRepName)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.partitionKey = 'customer' " +
            "AND CONTAINS(LOWER(c.salesRepresentative.name), LOWER(@name))")
            .WithParameter("@name", salesRepName);
        return await RunQueryAsync(query);
    }

    public async Task<Customer?> UpdateCustomerAsync(string id, Customer customer)
    {
        customer.Id = id;
        customer.UpdatedAt = DateTime.UtcNow;
        try
        {
            var response = await _container.ReplaceItemAsync(
                customer,
                id,
                new PartitionKey("customer"));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> DeleteCustomerAsync(string id)
    {
        try
        {
            await _container.DeleteItemAsync<Customer>(id, new PartitionKey("customer"));
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private async Task<IEnumerable<Customer>> RunQueryAsync(QueryDefinition query)
    {
        var results = new List<Customer>();
        using var iterator = _container.GetItemQueryIterator<Customer>(query);
        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync();
            results.AddRange(page);
        }
        return results;
    }
}