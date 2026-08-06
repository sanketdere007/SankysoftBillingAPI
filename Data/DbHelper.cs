using System.Data;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Data;

/// <summary>
/// Production-ready ADO.NET Database Helper class providing asynchronous execution
/// of SQL Commands, Stored Procedures, and SqlDataReader mappings.
/// </summary>
public class DbHelper
{
    private readonly string _connectionString;
    private readonly ILogger<DbHelper> _logger;

    public DbHelper(IConfiguration configuration, ILogger<DbHelper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string 'DefaultConnection' was not found in configuration.");
    }

    /// <summary>
    /// Creates and opens a new SqlConnection asynchronously.
    /// </summary>
    public async Task<SqlConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    /// <summary>
    /// Executes a Stored Procedure that takes a JSON parameter and returns a SqlDataReader.
    /// Used specifically for SP_Employee_InsertOrUpdate and other JSON-based procedures.
    /// </summary>
    public async Task<T> ExecuteStoredProcedureWithJsonAsync<T>(
        string procedureName,
        string jsonParameterName,
        string jsonData,
        Func<SqlDataReader, Task<T>> mapReaderFunc,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await GetOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        var parameter = new SqlParameter(jsonParameterName, SqlDbType.NVarChar, -1)
        {
            Value = string.IsNullOrWhiteSpace(jsonData) ? (object)DBNull.Value : jsonData
        };
        command.Parameters.Add(parameter);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken);
        return await mapReaderFunc(reader);
    }

    /// <summary>
    /// Executes a stored procedure with custom parameters and maps the SqlDataReader to a result.
    /// </summary>
    public async Task<T> ExecuteStoredProcedureAsync<T>(
        string procedureName,
        IEnumerable<SqlParameter>? parameters,
        Func<SqlDataReader, Task<T>> mapReaderFunc,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await GetOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }
        }

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken);
        return await mapReaderFunc(reader);
    }

    /// <summary>
    /// Executes a SQL text query with parameters and maps the SqlDataReader to a result list or entity.
    /// </summary>
    public async Task<T> ExecuteReaderAsync<T>(
        string queryText,
        CommandType commandType,
        IEnumerable<SqlParameter>? parameters,
        Func<SqlDataReader, Task<T>> mapReaderFunc,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await GetOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(queryText, connection)
        {
            CommandType = commandType,
            CommandTimeout = 30
        };

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }
        }

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken);
        return await mapReaderFunc(reader);
    }

    /// <summary>
    /// Executes a command returning a scalar value asynchronously.
    /// </summary>
    public async Task<object?> ExecuteScalarAsync(
        string commandText,
        CommandType commandType,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await GetOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(commandText, connection)
        {
            CommandType = commandType,
            CommandTimeout = 30
        };

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }
        }

        return await command.ExecuteScalarAsync(cancellationToken);
    }

    /// <summary>
    /// Executes a command returning number of rows affected.
    /// </summary>
    public async Task<int> ExecuteNonQueryAsync(
        string commandText,
        CommandType commandType,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await GetOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(commandText, connection)
        {
            CommandType = commandType,
            CommandTimeout = 30
        };

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }
        }

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Helper to create an input SqlParameter with proper DBNull conversion.
    /// </summary>
    public static SqlParameter CreateParameter(string parameterName, object? value, SqlDbType dbType, int size = 0)
    {
        var param = size > 0 
            ? new SqlParameter(parameterName, dbType, size) 
            : new SqlParameter(parameterName, dbType);

        param.Value = value ?? DBNull.Value;
        param.Direction = ParameterDirection.Input;
        return param;
    }
}
