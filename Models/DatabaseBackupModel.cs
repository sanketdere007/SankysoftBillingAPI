namespace Billing_Software_Api.Models;

/// <summary>
/// Model representing the database backup operation result and file metadata.
/// </summary>
public class DatabaseBackupModel
{
    /// <summary>
    /// Status indicating whether the backup operation succeeded (true/1) or failed (false/0).
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// Informational message returned by the stored procedure or repository.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Full server file path where the .bak backup file was saved.
    /// </summary>
    public string BackupFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Name of the backup file (e.g., SankysoftBillingDB_20260806_192001.bak).
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Size of the backup file in bytes (if accessible on the file system).
    /// </summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// Human-readable formatted file size (e.g., "15.42 MB").
    /// </summary>
    public string? FileSizeFormatted { get; set; }

    /// <summary>
    /// Timestamp when the backup was generated.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
