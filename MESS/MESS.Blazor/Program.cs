using MESS.Blazor.Components;
using MESS.Data.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>(options =>
{
    var env = builder.Environment;

    // Use SQLite3 server when in development
    if (!env.IsDevelopment())
    {
        options.UseSqlServer();
    }
    else
    {
        options.UseSqlite("Data Source=mydatabase.db");
    }
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// User Secrets Setup
var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

// Example user secrets usage
// Console.WriteLine($"Hello, {config["Name"]}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();