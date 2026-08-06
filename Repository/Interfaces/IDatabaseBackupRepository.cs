using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

/// <summary>
/// Repository interface for Database Backup operations.
/// </summary>
public interface IDatabaseBackupRepository
{
    /// <summary>
    /// Executes SP_DatabaseBackup to trigger full database backup and returns metadata.
    /// </summary>
    Task<ApiResponse<DatabaseBackupModel>> CreateDatabaseBackupAsync(CancellationToken cancellationToken = default);
}
