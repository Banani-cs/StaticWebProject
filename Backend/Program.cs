using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Registering the database context, UseSqlServer connects the EF Core to the SQL Server, read the connection string from the appsettings.json file, and configure the DbContext with the options provided.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapPost("/currency", async (CurrencyRequest request, ApplicationDbContext dbContext) =>
{
    if (request == null || string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest(new { Message = "Invalid request. 'Name' is required." });
    }

    string cleanName = request.Name.Trim();

    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Name == cleanName);
    if (user != null)
    {
        user.Currency += user.ClickPower; // Increment currency by ClickPower
        await dbContext.SaveChangesAsync();
    }
    else
    {
        user = new User { Name = cleanName, Currency = 0, ClickPower = 1, UpgradeCost = 10 }; // Initialize ClickPower and UpgradeCost
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
    }
    return TypedResults.Ok(new { Name = cleanName, CurrencyCount = user.Currency, ClickPower = user.ClickPower, UpgradeCost = user.UpgradeCost });
});

app.MapPost("/currency/spend", async (ApplicationDbContext dbContext, CurrencyRequest request, int amount) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Name == request.Name);
    if (user == null)
    {
        return Results.NotFound(new { Message = $"User with '{request.Name}' not found." });
    }
    if (amount < 0)
    {
        return Results.BadRequest(new { Message = "Amount to spend must be greater than or equal to 0." });
    }
    if (user != null && user.Currency >= amount)
    {
        user.Currency -= amount;
        user.ClickPower += 1; // Increase ClickPower by 1 for each spend
        user.UpgradeCost += 3; // Increase UpgradeCost by 3 for each spend
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok(new { Name = request.Name, CurrencyCount = user.Currency });
    }
    else
    {
        return Results.BadRequest(new { Message = $"Insufficient currency for '{request.Name}'" });
    }
});

app.MapGet("/currency/{name}/info", async (string name, ApplicationDbContext dbContext) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Name == name);
    if (user == null)
    {
        return Results.NotFound(new { Message = $"User '{name}' not found." });
    }
    return TypedResults.Ok(new { Name = name, CurrencyCount = user.Currency });
});

app.MapGet("/currency/return_all_users", async (ApplicationDbContext dbContext) =>
{
    return TypedResults.Ok(await dbContext.Users.Select(u => new { u.Name, u.Currency, u.ClickPower, u.UpgradeCost }).ToListAsync());
    // return TypedResults.Ok(currencyDict.Select(kvp => new { Name = kvp.Key }));
});

app.MapGet("/currency/return_all_currency", async (ApplicationDbContext dbContext) =>
{
    return TypedResults.Ok(await dbContext.Users.Select(u => new { u.Currency }).ToListAsync());
});

app.MapPut("/currency/{name}/update", async (string name, ApplicationDbContext dbContext, int amount) =>
{
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Name == name);
    if (user == null)
    {
        return Results.NotFound(new { Message = $"User '{name}' not found." });
    }
    if (user != null)
    {
        user.Currency = amount;
        await dbContext.SaveChangesAsync();
        return TypedResults.Ok(new { Amount = amount, Message = "added for", Name = name, CurrencyCount = user.Currency });
    }
    return Results.NotFound(new { Message = $"Currency '{name}' not found." });
});
app.Run();
public class CurrencyRequest
{
    public required string Name { get; set; }
}