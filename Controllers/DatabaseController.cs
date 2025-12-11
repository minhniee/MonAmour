using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonAmour.Models;

namespace MonAmour.Controllers
{
    /// <summary>
    /// Controller for database management operations
    /// </summary>
    [ApiController]
    [Route("database")]
    public class DatabaseController : ControllerBase
    {
        private readonly MonAmourDbContext _context;
        private readonly ILogger<DatabaseController> _logger;
        private readonly IConfiguration _configuration;

        public DatabaseController(
            MonAmourDbContext context,
            ILogger<DatabaseController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Apply pending database migrations
        /// POST /database/migrate
        /// </summary>
        [HttpPost("migrate")]
        public async Task<IActionResult> Migrate()
        {
            try
            {
                _logger.LogInformation("=== STARTING DATABASE MIGRATION ===");

                // Check if database can connect
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogError("Cannot connect to database");
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cannot connect to database",
                        timestamp = DateTime.UtcNow
                    });
                }

                // Get pending migrations
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                var pendingList = pendingMigrations.ToList();

                if (!pendingList.Any())
                {
                    _logger.LogInformation("No pending migrations");
                    return Ok(new
                    {
                        success = true,
                        message = "No pending migrations",
                        appliedMigrations = new string[0],
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("Found {Count} pending migrations: {Migrations}",
                    pendingList.Count,
                    string.Join(", ", pendingList));

                // Apply migrations
                await _context.Database.MigrateAsync();

                _logger.LogInformation("✅ Successfully applied {Count} migrations", pendingList.Count);

                // Get applied migrations after migration
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Successfully applied {pendingList.Count} migration(s)",
                    appliedMigrations = pendingList,
                    totalAppliedMigrations = appliedMigrations.Count(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error applying migrations: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error applying migrations",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get migration status
        /// GET /database/migrations/status
        /// </summary>
        [HttpGet("migrations/status")]
        public async Task<IActionResult> GetMigrationStatus()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Cannot connect to database",
                        timestamp = DateTime.UtcNow
                    });
                }

                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();

                return Ok(new
                {
                    success = true,
                    appliedMigrations = appliedMigrations.ToList(),
                    pendingMigrations = pendingMigrations.ToList(),
                    appliedCount = appliedMigrations.Count(),
                    pendingCount = pendingMigrations.Count(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting migration status: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error getting migration status",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}

