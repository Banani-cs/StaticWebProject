var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
Dictionary<string, int> currencyDict = new Dictionary<string, int>();

app.MapPost("/currency", (CurrencyRequest request) =>
{
    if (currencyDict.ContainsKey(request.Name))
    {
        currencyDict[request.Name]++;
    }
    else
    {
        currencyDict[request.Name] = 1;
    }
    return TypedResults.Ok(new { Name = request.Name, CurrencyCount = currencyDict[request.Name] });
});

app.MapGet("/currency/{name}", (string name) =>
{
    if (!currencyDict.TryGetValue(name, out int count))
    {
        return Results.NotFound(new { Message = $"Currency '{name}' not found." });
    }
    return TypedResults.Ok(new { Name = name, CurrencyCount = count });
});
app.Run();
public class CurrencyRequest
{
    public string Name { get; set; }
}