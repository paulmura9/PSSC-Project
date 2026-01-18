using Microsoft.Data.SqlClient;

var connectionString = "Server=pssc-server.database.windows.net;Database=pssc-db;User Id=CloudSAa6035330;Password=pssc2026!;Encrypt=True;TrustServerCertificate=True;";

try
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    Console.WriteLine("Connected to database!");

    var migrations = new[]
    {
        "IF COL_LENGTH('ordering.Orders', 'PickupMethod') IS NULL ALTER TABLE [ordering].[Orders] ADD [PickupMethod] NVARCHAR(32) NOT NULL DEFAULT 'HomeDelivery'",
        "IF COL_LENGTH('ordering.Orders', 'PickupPointId') IS NULL ALTER TABLE [ordering].[Orders] ADD [PickupPointId] NVARCHAR(64) NULL",
        "IF COL_LENGTH('ordering.Orders', 'PaymentMethod') IS NULL ALTER TABLE [ordering].[Orders] ADD [PaymentMethod] NVARCHAR(32) NOT NULL DEFAULT 'CashOnDelivery'",
        "IF COL_LENGTH('ordering.Orders', 'PaymentStatus') IS NULL ALTER TABLE [ordering].[Orders] ADD [PaymentStatus] NVARCHAR(32) NOT NULL DEFAULT 'Pending'"
    };

    foreach (var sql in migrations)
    {
        using var cmd = new SqlCommand(sql, connection);
        var result = await cmd.ExecuteNonQueryAsync();
        Console.WriteLine($"Executed: {sql.Substring(0, Math.Min(50, sql.Length))}... Result: {result}");
    }

    // Verify columns
    Console.WriteLine("\nColumns in ordering.Orders:");
    using var verifyCmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'ordering' AND TABLE_NAME = 'Orders' ORDER BY ORDINAL_POSITION", connection);
    using var reader = await verifyCmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        Console.WriteLine($"  - {reader["COLUMN_NAME"]}");
    }

    Console.WriteLine("\nMigration completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

