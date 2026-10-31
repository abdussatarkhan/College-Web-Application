using College_Web_Application.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection & Context Configuration (Resilient EF Core)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
bool useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", false) 
                   || string.IsNullOrWhiteSpace(connectionString) 
                   || connectionString.Equals("InMemory", StringComparison.OrdinalIgnoreCase);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (!useInMemory && !string.IsNullOrEmpty(connectionString) && (connectionString.Contains("Host=") || connectionString.Contains("Server=")))
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
        });
    }
    else
    {
        // High-concurrency InMemory database for zero-config local run
        options.UseInMemoryDatabase("GdcMadyanSwat_CollegePortalDb");
    }
});

// 2. ASP.NET Core Identity with Role-Based Access Control (RBAC)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password Policies (Designed for Student CNICs & Staff Credentials)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User & Sign-In Policies
    options.User.RequireUniqueEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;

    // Account Lockout Policies for Security
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Cookie Authentication Settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "GDCMadyan.AuthCookie";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(10);
    options.SlidingExpiration = true;
});

// 4. Memory Caching for Peak Traffic (Admissions & Result Days)
builder.Services.AddMemoryCache();

// 5. Response Compression (Brotli & Gzip for High Performance on Mobile 4G)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// 6. Institutional Services (Attendance & Compliance Engines)
builder.Services.AddScoped<College_Web_Application.Services.IAttendanceCalculatorService, College_Web_Application.Services.AttendanceCalculatorService>();
builder.Services.AddScoped<College_Web_Application.Services.IParentNotificationService, College_Web_Application.Services.ParentNotificationService>();
builder.Services.AddScoped<College_Web_Application.Services.IAttendanceComplianceService, College_Web_Application.Services.AttendanceComplianceService>();

// 7. MVC Controllers and Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 7. HTTP Request Pipeline Configuration
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Area Routes for Admin, Faculty, and Student Management Portals
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default Public Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 8. Automated Startup Database Migration & Identity Seeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Initializing GDC Madyan Swat institutional database and Identity roles...");
        await DbInitializer.SeedAsync(services);
        logger.LogInformation("GDC Madyan Swat database initialization complete. System ready.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration or role seeding on startup.");
    }
}

app.Run();
