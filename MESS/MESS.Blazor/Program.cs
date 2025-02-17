using MESS.Blazor.Components;
using MESS.Data.Context;
using MESS.Services.Product;
using MESS.Data.Seed;
using MESS.Services.ProductionLog;
using MESS.Services.WorkInstruction;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MESSConnection");
    
    options.UseSqlServer(connectionString);

});

builder.Services.AddTransient<IWorkInstructionService, WorkInstructionService>();
builder.Services.AddTransient<IProductionLogService, ProductionLogService>();
builder.Services.AddHttpClient();

// Register the ProductService
builder.Services.AddTransient<IProductService, ProductService>();

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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedWorkInstructions.Seed(services);
}
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