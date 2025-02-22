using MESS.Blazor.Components;
using MESS.Data.Context;
using MESS.Data.Seed;
using MESS.Services.ProductionLog;
using MESS.Services.WorkInstruction;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MESSConnection");
    
    options.UseSqlServer(connectionString);

});

builder.Services.AddTransient<IWorkInstructionService, WorkInstructionService>();
builder.Services.AddTransient<IProductionLogService, ProductionLogService>();
builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var logLevel = builder.Environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Warning;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(new JsonFormatter(), "Logs/MESS_Blazor_Warning_Log.json", restrictedToMinimumLevel: LogEventLevel.Warning)
    .WriteTo.File("Logs/MESS_Blazor_All.logs",
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}}",
        rollingInterval: RollingInterval.Day)
    .MinimumLevel.Is(logLevel)
    .CreateLogger();


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