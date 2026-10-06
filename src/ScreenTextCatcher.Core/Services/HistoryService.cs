using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;
using ScreenTextCatcher.Core.Models;

namespace ScreenTextCatcher.Core.Services;

public class HistoryService : IHistoryService
{
    private readonly string _connectionString;
    private readonly object _lock = new();

    public HistoryService(string? dbPath = null)
    {
        if (string.IsNullOrWhiteSpace(dbPath))
        {
            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ScreenTextCatcher");
            Directory.CreateDirectory(appDataDir);
            dbPath = Path.Combine(appDataDir, "history.db");
        }
        else
        {
            var dir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        lock (_lock)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS History (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp TEXT NOT NULL,
                    Text TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_History_Id ON History(Id DESC);
            ";
            command.ExecuteNonQuery();
        }
    }

    public void Add(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        lock (_lock)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            using var insertCmd = connection.CreateCommand();
            insertCmd.Transaction = transaction;
            insertCmd.CommandText = "INSERT INTO History (Timestamp, Text) VALUES ($ts, $txt);";
            insertCmd.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("O"));
            insertCmd.Parameters.AddWithValue("$txt", text);
            insertCmd.ExecuteNonQuery();

            // Purge beyond the newest 100 items
            using var pruneCmd = connection.CreateCommand();
            pruneCmd.Transaction = transaction;
            pruneCmd.CommandText = "DELETE FROM History WHERE Id NOT IN (SELECT Id FROM History ORDER BY Id DESC LIMIT 100);";
            pruneCmd.ExecuteNonQuery();

            transaction.Commit();
        }
    }

    public IReadOnlyList<HistoryItem> GetRecent(int limit = 100)
    {
        lock (_lock)
        {
            var items = new List<HistoryItem>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Timestamp, Text FROM History ORDER BY Id DESC LIMIT $limit;";
            cmd.Parameters.AddWithValue("$limit", limit);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetInt64(0);
                var tsStr = reader.GetString(1);
                var text = reader.GetString(2);

                if (!DateTime.TryParse(tsStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var ts))
                {
                    ts = DateTime.UtcNow;
                }

                items.Add(new HistoryItem(id, ts.ToLocalTime(), text));
            }

            return items;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM History;";
            cmd.ExecuteNonQuery();
        }
    }

    public int Count()
    {
        lock (_lock)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM History;";
            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }
    }
}
