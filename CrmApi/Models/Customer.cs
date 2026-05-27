using Newtonsoft.Json;

namespace CrmApi.Models;

public class Customer
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;

    [JsonProperty("address")]
    public string Address { get; set; } = string.Empty;

    [JsonProperty("salesRepresentative")]
    public SalesRepresentative SalesRepresentative { get; set; } = new();

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("partitionKey")]
    public string PartitionKey { get; set; } = "customer";
}

public class SalesRepresentative
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;
}