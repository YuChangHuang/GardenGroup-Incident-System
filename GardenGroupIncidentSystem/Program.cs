using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using GardenGroupIncidentSystem.Services;
using GardenGroupIncidentSystem.Services.Filtering;
using GardenGroupIncidentSystem.Services.Repositories;
using GardenGroupIncidentSystem.Services.Sorting;
using MongoDB.Driver;

// Load .env file FIRST
try
{
    DotNetEnv.Env.TraversePath().Load();
}
catch
{
    // If .env not found, connection string should be in appsettings.json
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
//creating session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// Register MongoClient as SINGLETON
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var conn = builder.Configuration["MongoDB:ConnectionString"];
    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException("MongoDB:ConnectionString not configured");

    return new MongoClient(conn);
});

// Register IMongoDatabase as SCOPED
builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var dbName = builder.Configuration["MongoDB:DatabaseName"];
    if (string.IsNullOrWhiteSpace(dbName))
        throw new InvalidOperationException("MongoDB:DatabaseName not configured");

    return client.GetDatabase(dbName);
});

// Register Repository (Data Access Layer)
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();


// Register Service (Business Logic Layer)
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<TicketArchivingService>();
builder.Services.AddScoped<ITicketSorter, TicketSorter>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<IKeywordFilterRepository, KeywordFilterRepository>();
builder.Services.AddScoped<IKeywordFilterService, KeywordFilterService>();


// Register Password Reset Services (YuChang Huang Individual Functionality)
builder.Services.AddHttpContextAccessor(); 
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<PasswordResetTokenService>();
builder.Services.AddScoped<PasswordResetService>();



var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession(); // Enable session middleware

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}"); 

app.Run();