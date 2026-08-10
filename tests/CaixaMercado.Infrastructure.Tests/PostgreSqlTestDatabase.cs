using System.Text.RegularExpressions;
using Npgsql;

namespace CaixaMercado.Infrastructure.Tests;

internal static partial class PostgreSqlTestDatabase
{
    public const string AdminConnectionEnvironmentVariable = "CAIXA_MERCADO_TEST_ADMIN";

    public static async Task ExecutarAsync(Func<string, Task> teste)
    {
        var adminConnectionString = Environment.GetEnvironmentVariable(AdminConnectionEnvironmentVariable)
            ?? throw new InvalidOperationException($"A variável {AdminConnectionEnvironmentVariable} não foi definida.");

        var databaseName = $"caixa_mercado_it_{Guid.NewGuid():N}";
        var databaseConnectionString = BuildDatabaseConnectionString(adminConnectionString, databaseName);
        await CreateDatabaseAsync(adminConnectionString, databaseName);

        try
        {
            await teste(databaseConnectionString);
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await DropDatabaseAsync(adminConnectionString, databaseName);
        }
    }

    private static string BuildDatabaseConnectionString(string adminConnectionString, string databaseName)
    {
        var builder = new NpgsqlConnectionStringBuilder(adminConnectionString)
        {
            Database = databaseName,
            Pooling = false
        };

        return builder.ConnectionString;
    }

    private static async Task CreateDatabaseAsync(string adminConnectionString, string databaseName)
    {
        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task DropDatabaseAsync(string adminConnectionString, string databaseName)
    {
        if (!IntegrationDatabaseName().IsMatch(databaseName))
            throw new InvalidOperationException("Nome de banco de integração recusado pela proteção de exclusão.");

        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }

    [GeneratedRegex("^caixa_mercado_it_[a-f0-9]{32}$", RegexOptions.CultureInvariant)]
    private static partial Regex IntegrationDatabaseName();
}

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(PostgreSqlTestDatabase.AdminConnectionEnvironmentVariable)))
        {
            Skip = $"Defina {PostgreSqlTestDatabase.AdminConnectionEnvironmentVariable} com uma conexão administrativa para executar o teste PostgreSQL real.";
        }
    }
}
