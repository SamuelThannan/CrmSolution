using Microsoft.Azure.Cosmos;
using CrmApi.Services;
using CrmApi.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "CRM API",
        Version = "v1",
        Description = "Minimal Web API för CRM-hantering med Azure Cosmos DB"
    });
});

builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["CosmosDb:ConnectionString"]
        ?? throw new InvalidOperationException("CosmosDb:ConnectionString saknas.");
    return new CosmosClient(connectionString);
});

builder.Services.AddSingleton<CosmosDbService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapCustomerEndpoints();
app.Run();