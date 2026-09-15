using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Registering the database context, UseSqlServer connects the EF Core to thbge SQL Server, read from appsettings.json for the connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
Dictionary<string, int> currencyDict = new();

app.MapPost("/currency", (CurrencyRequest request) =>
{
    if (request == null || string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest(new { Message = "Invalid request. 'Name' is required." });
    }

    string cleanName = request.Name.Trim();

    {
        if (currencyDict.TryGetValue(cleanName, out int count))
        {
            currencyDict[cleanName]++;
        }
        else
        {
            currencyDict[cleanName] = 1;
        }
    }
    return TypedResults.Ok(new { Name = cleanName, CurrencyCount = currencyDict[cleanName] });
});

app.MapPost("/currency/spend", (CurrencyRequest request, int amount) =>
{
    if (amount < 0)
    {
        return Results.BadRequest(new { Message = "Amount to spend must be greater than or equal to 0." });
    }

    if (!currencyDict.ContainsKey(request.Name))
    {
        return Results.NotFound(new { Message = $"User with '{request.Name}' not found." });
    }
    if (currencyDict.TryGetValue(request.Name, out int count) && count >= amount)
    {
        currencyDict[request.Name] -= amount;
        return TypedResults.Ok(new { Name = request.Name, CurrencyCount = currencyDict[request.Name] });
    }
    else
    {
        return Results.BadRequest(new { Message = $"Insufficient currency for '{request.Name}'" });
    }
});


app.MapGet("/currency/{name}/info", (string name) =>
{
    if (!currencyDict.TryGetValue(name, out int count))
    {
        return Results.NotFound(new { Message = $"Currency '{name}' not found." });
    }
    return TypedResults.Ok(new { Name = name, CurrencyCount = count });
});

app.MapGet("/currency/return_all_users", () =>
{
    return TypedResults.Ok(currencyDict.Select(kvp => new { Name = kvp.Key }));
});

app.MapGet("/currency/return_all_currency", () =>
{
    return TypedResults.Ok(currencyDict.Select(kvp => new { CurrencyCount = kvp.Value }));
});

app.MapPut("/currency/{name}/update", (string name) =>
{
    if (currencyDict.ContainsKey(name))
    {
        currencyDict[name] = 1000;
        return TypedResults.Ok(new { Name = name, CurrencyCount = currencyDict[name] });
    }
    return Results.NotFound(new { Message = $"Currency '{name}' not found." });
});
app.Run();
public class CurrencyRequest
{
    public required string Name { get; set; }
}