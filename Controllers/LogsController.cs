using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Models;

namespace MonAmour.Controllers
{
    public class LogsController : Controller
    {
        private readonly ILogger<LogsController> _logger;
        private readonly MonAmourDbContext _context;

        public LogsController(ILogger<LogsController> logger, MonAmourDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("logs/startup")]
        public IActionResult StartupLogs()
        {
            try
            {
                // Generate some startup log entries
                _logger.LogInformation("📋 Startup logs requested at {Time}", DateTime.UtcNow);
                _logger.LogInformation("Current Environment: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
                _logger.LogInformation("Machine Name: {MachineName}", Environment.MachineName);
                _logger.LogInformation("Current Directory: {Directory}", Directory.GetCurrentDirectory());
                _logger.LogInformation("Process ID: {ProcessId}", Environment.ProcessId);

                return Json(new
                {
                    message = "Startup log entries generated. Check Azure App Service logs.",
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    machineName = Environment.MachineName,
                    processId = Environment.ProcessId,
                    currentDirectory = Directory.GetCurrentDirectory()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating startup logs");
                return Json(new
                {
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("logs/database-test")]
        public async Task<IActionResult> DatabaseTestLogs()
        {
            try
            {
                _logger.LogInformation("🔍 Starting comprehensive database test...");

                // Test 1: Connection
                var canConnect = await _context.Database.CanConnectAsync();
                _logger.LogInformation("Database Connection: {Status}", canConnect ? "✅ SUCCESS" : "❌ FAILED");

                if (!canConnect)
                {
                    _logger.LogError("Cannot connect to database. Stopping tests.");
                    return Json(new { error = "Cannot connect to database", timestamp = DateTime.UtcNow });
                }

                // Test 2: Migrations
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                
                _logger.LogInformation("Applied migrations: {Count}", appliedMigrations.Count());
                _logger.LogInformation("Pending migrations: {Count}", pendingMigrations.Count());

                foreach (var migration in appliedMigrations.Take(5))
                {
                    _logger.LogInformation("✅ Applied: {Migration}", migration);
                }

                foreach (var migration in pendingMigrations.Take(5))
                {
                    _logger.LogWarning("⏳ Pending: {Migration}", migration);
                }

                // Test 3: Table access
                var tableTests = new Dictionary<string, int>();

                try
                {
                    tableTests["Users"] = await _context.Users.CountAsync();
                    _logger.LogInformation("Users table: {Count} records", tableTests["Users"]);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error accessing Users table: {Error}", ex.Message);
                    tableTests["Users"] = -1;
                }

                try
                {
                    tableTests["Concepts"] = await _context.Concepts.CountAsync();
                    _logger.LogInformation("Concepts table: {Count} records", tableTests["Concepts"]);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error accessing Concepts table: {Error}", ex.Message);
                    tableTests["Concepts"] = -1;
                }

                try
                {
                    tableTests["Products"] = await _context.Products.CountAsync();
                    _logger.LogInformation("Products table: {Count} records", tableTests["Products"]);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error accessing Products table: {Error}", ex.Message);
                    tableTests["Products"] = -1;
                }

                try
                {
                    tableTests["Orders"] = await _context.Orders.CountAsync();
                    _logger.LogInformation("Orders table: {Count} records", tableTests["Orders"]);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error accessing Orders table: {Error}", ex.Message);
                    tableTests["Orders"] = -1;
                }

                _logger.LogInformation("🔍 Database test completed successfully");

                return Json(new
                {
                    success = true,
                    connectionStatus = canConnect,
                    appliedMigrationsCount = appliedMigrations.Count(),
                    pendingMigrationsCount = pendingMigrations.Count(),
                    tableTests = tableTests,
                    message = "Database test logs generated. Check Azure App Service logs for details.",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during database test: {Message}", ex.Message);
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("logs/system-info")]
        public IActionResult SystemInfo()
        {
            try
            {
                var systemInfo = new
                {
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    MachineName = Environment.MachineName,
                    ProcessId = Environment.ProcessId,
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    RuntimeVersion = Environment.Version.ToString(),
                    OSVersion = Environment.OSVersion.ToString(),
                    ProcessorCount = Environment.ProcessorCount,
                    Is64BitProcess = Environment.Is64BitProcess,
                    UserName = Environment.UserName,
                    CommandLine = Environment.CommandLine,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("🖥️ System Information Report:");
                _logger.LogInformation("Environment: {Environment}", systemInfo.Environment);
                _logger.LogInformation("Machine Name: {MachineName}", systemInfo.MachineName);
                _logger.LogInformation("Process ID: {ProcessId}", systemInfo.ProcessId);
                _logger.LogInformation("Working Directory: {Directory}", systemInfo.WorkingDirectory);
                _logger.LogInformation("Runtime Version: {Version}", systemInfo.RuntimeVersion);
                _logger.LogInformation("OS Version: {OS}", systemInfo.OSVersion);
                _logger.LogInformation("Processor Count: {Count}", systemInfo.ProcessorCount);
                _logger.LogInformation("64-bit Process: {Is64Bit}", systemInfo.Is64BitProcess);

                return Json(systemInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating system info");
                return Json(new
                {
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("logs/generate-test-log")]
        public IActionResult GenerateTestLog()
        {
            try
            {
                _logger.LogTrace("🔍 TRACE level log entry");
                _logger.LogDebug("🐛 DEBUG level log entry");
                _logger.LogInformation("ℹ️ INFORMATION level log entry");
                _logger.LogWarning("⚠️ WARNING level log entry");
                _logger.LogError("❌ ERROR level log entry");
                _logger.LogCritical("🚨 CRITICAL level log entry");

                return Json(new
                {
                    message = "Test log entries generated at all log levels",
                    timestamp = DateTime.UtcNow,
                    levels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test logs");
                return Json(new
                {
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
