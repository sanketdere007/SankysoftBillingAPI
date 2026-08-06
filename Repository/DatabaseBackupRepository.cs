using System.Data;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

/// <summary>
/// ADO.NET repository implementation for SQL Server database backup operations using SP_DatabaseBackup.
/// </summary>
public class DatabaseBackupRepository : IDatabaseBackupRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<DatabaseBackupRepository> _logger;

    public DatabaseBackupRepository(DbHelper dbHelper, ILogger<DatabaseBackupRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes SP_DatabaseBackup stored procedure to generate a full SQL Server database backup.
    /// </summary>
    public async Task<ApiResponse<DatabaseBackupModel>> CreateDatabaseBackupAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing stored procedure [dbo].[SP_DatabaseBackup] to trigger database backup...");

            // Ensure destination directory exists on disk so SQL Server does not fail
            const string defaultBackupDir = @"D:\Sankysoft\Backup";
            try
            {
                if (!Directory.Exists(defaultBackupDir))
                {
                    Directory.CreateDirectory(defaultBackupDir);
                    _logger.LogInformation("Created backup directory at: {Directory}", defaultBackupDir);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not pre-create backup directory at {Directory}. SQL Server might fail if directory does not exist.", defaultBackupDir);
            }

            var backupResult = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_DatabaseBackup",
                parameters: null,
                mapReaderFunc: async reader =>
                {
                    var model = new DatabaseBackupModel();
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        if (HasColumn(reader, "Status") && !reader.IsDBNull(reader.GetOrdinal("Status")))
                            model.Status = Convert.ToBoolean(reader["Status"]);

                        if (HasColumn(reader, "Message") && !reader.IsDBNull(reader.GetOrdinal("Message")))
                            model.Message = Convert.ToString(reader["Message"]) ?? string.Empty;

                        if (HasColumn(reader, "BackupFilePath") && !reader.IsDBNull(reader.GetOrdinal("BackupFilePath")))
                            model.BackupFilePath = Convert.ToString(reader["BackupFilePath"]) ?? string.Empty;
                    }
                    return model;
                },
                commandTimeout: 300, // 5 minutes timeout for database backup
                cancellationToken: cancellationToken);

            if (backupResult == null || string.IsNullOrWhiteSpace(backupResult.BackupFilePath))
            {
                _logger.LogWarning("SP_DatabaseBackup executed but did not return backup file details.");
                return ApiResponse<DatabaseBackupModel>.FailureResult(
                    message: "Database backup execution completed without returning file path details.",
                    error: "No output returned from stored procedure SP_DatabaseBackup.");
            }

            // Populate additional metadata from the generated file if available on the system
            backupResult.FileName = Path.GetFileName(backupResult.BackupFilePath);

            try
            {
                if (File.Exists(backupResult.BackupFilePath))
                {
                    var fileInfo = new FileInfo(backupResult.BackupFilePath);
                    backupResult.FileSizeBytes = fileInfo.Length;
                    backupResult.FileSizeFormatted = FormatBytes(fileInfo.Length);
                    backupResult.CreatedAt = fileInfo.CreationTimeUtc;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not inspect physical file attributes for: {Path}", backupResult.BackupFilePath);
            }

            _logger.LogInformation("Database backup generated successfully at: {Path}", backupResult.BackupFilePath);

            return ApiResponse<DatabaseBackupModel>.SuccessResult(
                data: backupResult,
                message: string.IsNullOrWhiteSpace(backupResult.Message) ? "Database backup created successfully." : backupResult.Message);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error during SP_DatabaseBackup execution: {Message}", sqlEx.Message);
            return ApiResponse<DatabaseBackupModel>.FailureResult(
                message: "Failed to create database backup due to a SQL Server error.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during database backup.");
            return ApiResponse<DatabaseBackupModel>.FailureResult(
                message: "An unexpected error occurred while creating the database backup.",
                error: ex.Message);
        }
    }

    private static bool HasColumn(SqlDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
