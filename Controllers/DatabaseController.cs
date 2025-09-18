using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Models;

namespace MonAmour.Controllers
{
    public class DatabaseController : Controller
    {
        private readonly MonAmourDbContext _context;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(MonAmourDbContext context, ILogger<DatabaseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("database/status")]
        public async Task<IActionResult> DatabaseStatus()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();

                return Json(new
                {
                    connectionStatus = canConnect ? "Connected" : "Disconnected",
                    appliedMigrations = appliedMigrations.ToList(),
                    pendingMigrations = pendingMigrations.ToList(),
                    appliedMigrationsCount = appliedMigrations.Count(),
                    pendingMigrationsCount = pendingMigrations.Count(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database status");
                return Json(new
                {
                    connectionStatus = "Error",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("database/migrate")]
        public async Task<IActionResult> RunMigrations()
        {
            try
            {
                _logger.LogInformation("Starting database migration...");
                
                // Check if database exists
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogError("Cannot connect to database");
                    return Json(new
                    {
                        success = false,
                        error = "Cannot connect to database",
                        timestamp = DateTime.UtcNow
                    });
                }

                // Get pending migrations
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                _logger.LogInformation($"Found {pendingMigrations.Count()} pending migrations");

                if (pendingMigrations.Any())
                {
                    await _context.Database.MigrateAsync();
                    _logger.LogInformation("Database migration completed successfully");

                    return Json(new
                    {
                        success = true,
                        message = "Migrations applied successfully",
                        appliedMigrations = pendingMigrations.ToList(),
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = true,
                        message = "No pending migrations to apply",
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running migrations");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("database/ensure-created")]
        public async Task<IActionResult> EnsureDatabaseCreated()
        {
            try
            {
                _logger.LogInformation("Ensuring database is created...");
                var created = await _context.Database.EnsureCreatedAsync();
                
                return Json(new
                {
                    success = true,
                    databaseCreated = created,
                    message = created ? "Database created successfully" : "Database already exists",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring database creation");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("database/tables")]
        public async Task<IActionResult> CheckTables()
        {
            try
            {
                var tables = new List<object>();

                // Check for main tables
                var userCount = await _context.Users.CountAsync();
                tables.Add(new { table = "Users", count = userCount });

                var conceptCount = await _context.Concepts.CountAsync();
                tables.Add(new { table = "Concepts", count = conceptCount });

                var productCount = await _context.Products.CountAsync();
                tables.Add(new { table = "Products", count = productCount });

                var orderCount = await _context.Orders.CountAsync();
                tables.Add(new { table = "Orders", count = orderCount });

                var blogCount = await _context.Blogs.CountAsync();
                tables.Add(new { table = "Blogs", count = blogCount });

                return Json(new
                {
                    success = true,
                    tables = tables,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking tables");
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
