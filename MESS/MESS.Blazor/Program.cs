using MESS.Blazor.Components;
using MESS.Data.Context;
using MESS.Data.Models;
using MESS.Data.Seed;
using MESS.Services.Product;
using MESS.Services.ApplicationUser;
using MESS.Services.BrowserCacheManager;
using MESS.Services.DarkMode;
using MESS.Services.LocalCacheManager;
using MESS.Services.ProductionLogServices;
using MESS.Services.Serialization;
using MESS.Services.SessionManager;
using MESS.Services.WorkInstruction;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<ApplicationContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MESSConnection");

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);

        // Use split queries to avoid Cartesian explosion when loading multiple collections
        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});

// Adding Separate DbContext for Identity
builder.Services.AddDbContext<UserContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MESSConnection");
    
    options.UseSqlServer(connectionString, options =>
    {
        options.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});

builder.Services.AddCascadingAuthenticationState();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
    
    
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWorkInstructionService, WorkInstructionService>();
builder.Services.AddScoped<IProductionLogService, ProductionLogService>();
builder.Services.AddScoped<ILocalCacheManager, LocalCacheManager>();
builder.Services.AddScoped<ISessionManager, SessionManager>();
builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<ISerializationService, SerializationService>();
builder.Services.AddScoped<IProductionLogEventService, ProductionLogEventService>();
builder.Services.AddScoped<IWorkInstructionEditorService, WorkInstructionEditorService>();
builder.Services.AddScoped<RoleInitializer>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddRazorPages();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password config
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    
    // Username
    options.User.RequireUniqueEmail = false;
    
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
});

// Adding Services for Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<UserContext>()
    .AddDefaultTokenProviders();

// Roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireTechnician", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("RequireOperator", policy =>
        policy.RequireRole("Operator"));
});

builder.Services.AddAntiforgery();

// Setup FluentUI
builder.Services.AddFluentUIComponents();

builder.Services.AddSingleton<DarkModeInstance>();



var logLevel = builder.Environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Warning;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/MESS_Blazor_Error.logs", restrictedToMinimumLevel: LogEventLevel.Error, rollingInterval: RollingInterval.Month)
    .WriteTo.File("Logs/MESS_Blazor_Warning.logs", restrictedToMinimumLevel: LogEventLevel.Warning, rollingInterval: RollingInterval.Day)
    .WriteTo.File("Logs/MESS_Blazor_All.logs",
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}}",
        rollingInterval: RollingInterval.Day)
    .MinimumLevel.Is(logLevel)
    .CreateLogger();

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    // Initializes the roles if they are not already created in the database
    var roleInit = scope.ServiceProvider.GetRequiredService<RoleInitializer>();
    await roleInit.InitializeAsync();
    
    // Seed default technician
    await InitialUserSeed.SeedDefaultUserAsync(scope.ServiceProvider);
    
    // Seeds default data
    SeedWorkInstructions.Seed(scope.ServiceProvider);
}



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapRazorPages();

app.MapControllers();

app.Run();