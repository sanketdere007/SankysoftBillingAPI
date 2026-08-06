using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

/// <summary>
/// Database Backup Controller managing automated SQL Server database backups.
/// Executes stored procedure [dbo].[SP_DatabaseBackup].
/// Protected by JWT Authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DatabaseBackupController : ControllerBase
{
    private readonly IDatabaseBackupRepository _backupRepository;
    private readonly ILogger<DatabaseBackupController> _logger;

    public DatabaseBackupController(IDatabaseBackupRepository backupRepository, ILogger<DatabaseBackupController> logger)
    {
        _backupRepository = backupRepository ?? throw new ArgumentNullException(nameof(backupRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create Database Backup (using SP_DatabaseBackup)
    /// Triggers full backup of [SankysoftBillingDB] database to 'D:\Sankysoft\Backup\' with timestamped filename.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status, Message, and BackupFilePath</returns>
    [HttpPost("CreateDatabaseBackup")]
    [ProducesResponseType(typeof(ApiResponse<DatabaseBackupModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DatabaseBackupModel>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateDatabaseBackup(CancellationToken cancellationToken = default)
    {
        var result = await _backupRepository.CreateDatabaseBackupAsync(cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
