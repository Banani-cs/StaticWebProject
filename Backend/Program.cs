var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
Dictionary<string, int> currencyDict = new Dictionary<string, int>();

app.MapPost("/currency", (CurrencyRequest request) =>
{
    {
        if (currencyDict.ContainsKey(request.Name))
        {
            currencyDict[request.Name]++;
        }
        else
        {
            currencyDict[request.Name] = 1;
        }
    }
    return TypedResults.Ok(new { Name = request.Name, CurrencyCount = currencyDict[request.Name] });
});

app.MapGet("/currency/{name}/info", (string name) =>
{
    if (!currencyDict.TryGetValue(name, out int count))
    {
        return Results.NotFound(new { Message = $"Currency '{name}' not found." });
    }
    return TypedResults.Ok(new { Name = name, CurrencyCount = count });
});

app.MapGet("/currency/return_all", () =>
{
    return TypedResults.Ok(currencyDict.Select(kvp => new { Name = kvp.Key, CurrencyCount = kvp.Value }));
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