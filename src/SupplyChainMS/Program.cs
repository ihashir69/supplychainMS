// ============================================================
// Program.cs — The application entry point
// ============================================================
//
// This is where the ASP.NET app starts up. It does TWO things:
//
//   PART 1 — Register Services ("builder" section)
//   This is like a configuration file that says:
//   "Here's what my app needs — give me a database connection,
//    give me authentication, give me MVC controllers..."
//   This is called the "Dependency Injection Container" (DI container).
//
//   PART 2 — Configure the HTTP Pipeline ("app" section)
//   This defines what happens to EVERY incoming request in order.
//   It's like a series of checkpoints a request must pass through:
//   authentication check → routing → controller → response
//
// In Flask/Django terms: this is like your app factory + middleware setup.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupplyChainMS.Data;
using SupplyChainMS.Models;
using SupplyChainMS.Services;
using SupplyChainMS.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// PART 1: Register Services
// ============================================================

// --- Database ---
// Tell EF Core: use PostgreSQL, and get the connection string
// from appsettings.json under "ConnectionStrings:DefaultConnection"
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- ASP.NET Identity (Authentication) ---
// This sets up the full user management system:
//   - Password hashing
//   - Login/logout
//   - Role management
//   - Cookie-based sessions
//
// AddDefaultTokenProviders() adds support for password reset tokens,
// email confirmation tokens, etc.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password rules — keeping it simple for a university project
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;

        // If a user tries to login with a wrong password 5 times,
        // lock their account for 5 minutes
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

        // We're using email as the username
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()   // store users in our PostgreSQL DB
    .AddDefaultTokenProviders();

// --- Cookie Configuration ---
// After login, the user gets a cookie stored in their browser.
// This cookie is how the server knows "this request comes from a logged-in user."
builder.Services.ConfigureApplicationCookie(options =>
{
    // If a non-logged-in user tries to access a protected page, redirect here
    options.LoginPath = "/Account/Login";

    // If a logged-in user tries to access a page they're not authorized for
    options.AccessDeniedPath = "/Account/AccessDenied";

    // Cookie expires after 7 days of inactivity (sliding expiry)
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// --- Application Services ---
// "AddScoped" means: create ONE instance of this service per HTTP request.
// The request comes in → service is created → request finishes → service is disposed.
// This is the correct lifetime for services that use DbContext (which is also scoped).
//
// The pattern: whenever a controller asks for ISupplierService,
// ASP.NET gives it a SupplierService instance automatically.
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// --- MVC Controllers + Razor Views ---
// This registers the MVC pattern: Controllers handle requests,
// Views (Razor .cshtml files) render the HTML response.
builder.Services.AddControllersWithViews();

// ============================================================
// PART 2: Build the app and configure the HTTP pipeline
// ============================================================

var app = builder.Build();

// --- Seed the database on startup ---
// We run seed data BEFORE the app starts accepting requests.
// This creates roles and test users if they don't exist.
//
// "using" here creates a temporary "scope" for DI services.
// We need this because DbContext is "scoped" (one per request),
// but we're running this OUTSIDE of a request context.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedData.InitializeAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        // Log the error but don't crash the app
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// --- Error Handling ---
if (!app.Environment.IsDevelopment())
{
    // In production, show a friendly error page instead of stack traces
    app.UseExceptionHandler("/Home/Error");
    // Force HTTPS in production
    app.UseHsts();
}
else
{
    // In development, show the full error with stack trace
    app.UseDeveloperExceptionPage();
}

// --- HTTPS Redirect ---
app.UseHttpsRedirection();

// --- Static Files ---
// This serves files from the wwwroot/ folder (CSS, JS, images).
// Without this, Bootstrap and your CSS wouldn't load.
app.UseStaticFiles();

// --- Routing ---
app.UseRouting();

// --- Authentication & Authorization ---
// IMPORTANT: These MUST come in this order — Authentication before Authorization.
// UseAuthentication: "Who is this user?" (reads the login cookie)
// UseAuthorization: "Are they allowed to do this?" (checks roles)
app.UseAuthentication();
app.UseAuthorization();

// --- Route Pattern ---
// This is the default URL pattern: /Controller/Action/Id
// Example: /Suppliers/Edit/5  → SuppliersController.Edit(id: 5)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
