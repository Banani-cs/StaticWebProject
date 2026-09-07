var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

int currency = 0;
app.MapGet("/currency/{name}", (string name) =>
{
    currency++;
    return $"Hello {name}!, you have {currency} currency(ies).";
});

app.Run();