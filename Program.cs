using Microsoft.EntityFrameworkCore;
using MonAmour.Filters;
using MonAmour.Middleware;
using MonAmour.Models;
using MonAmour.Services.Implements;
using MonAmour.Services.Interfaces;
using MonAmour.Util;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<UserMenuFilter>();
});

// Add Entity Framework
builder.Services.AddDbContext<MonAmourDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(1); // Kéo dài thời gian session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Cho phép HTTP trong development
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Authorization (using custom SessionAuthorizeAttribute)
builder.Services.AddAuthorization();

// Add Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICassoService, CassoService>();
builder.Services.AddScoped<IVietQRService, VietQRService>();

builder.Services.AddScoped<IWishListService, WishListService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IBlogManagementService, BlogManagementService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IConceptService, ConceptService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Add SignalR
builder.Services.AddSignalR();


// Add HttpClient for Casso API
builder.Services.AddHttpClient<ICassoService, CassoService>();

// Add HttpClient for VietQR API
builder.Services.AddHttpClient<IVietQRService, VietQRService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["VietQR:ApiBase"] ?? "https://api.vietqr.io");
});


var app = builder.Build();

// Initialize system roles and settings
using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        await authService.InitializeSystemAsync();
        logger.LogInformation("System initialized successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize system");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add Session middleware
app.UseSession();

// Add Authentication middleware
app.UseAuthentication();

// Add Remember Me middleware
app.UseMiddleware<RememberMeMiddleware>();

app.UseAuthorization();

// Add 404 handling middleware
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map Error routes
app.MapControllerRoute(
    name: "error",
    pattern: "Error/{statusCode}",
    defaults: new { controller = "Error", action = "HttpStatusCodeHandler" });

app.MapControllerRoute(
    name: "notfound",
    pattern: "NotFound",
    defaults: new { controller = "Error", action = "NotFound" });

// Map SignalR Hub
app.MapHub<MonAmour.Hubs.CommentHub>("/commentHub");


app.Run();
