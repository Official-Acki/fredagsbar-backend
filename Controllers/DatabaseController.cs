using Npgsql;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;

public class DatabaseController
{
    // Singleton instance with only a get method, instanciating itself.
    public static DatabaseController Instance { get; } = new DatabaseController();
    public static float SESSION_VALIDITY_DAYS = Environment.GetEnvironmentVariable("SESSION_VALIDITY_DAYS") != null ? float.Parse(Environment.GetEnvironmentVariable("SESSION_VALIDITY_DAYS")!) : 7;
    public string ConnectionString { get; }
    public IDbConnection db => new NpgsqlConnection(ConnectionString);

    public DatabaseController()
    {
        NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder()
        {
            Host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost",
            Port = int.Parse(Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"),
            Username = Environment.GetEnvironmentVariable("DB_USER") ?? "localhost",
            Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "localhost",
            Database = Environment.GetEnvironmentVariable("DB_NAME") ?? "fredagsbar-backend",
            Pooling = true,
            MinPoolSize = 2,
            MaxPoolSize = 20,
        };
        
        this.ConnectionString = builder.ConnectionString;
    }

    public bool TestConnection()
    {
        try
        {
            using (var connection = db)
            {
                connection.Open();
                return connection.State == ConnectionState.Open;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal int ApplyMigrations()
    {
        while(TestConnection() == false)
        {
            // Wait for the db to be ready
            System.Threading.Thread.Sleep(5000);
        }
        string[] files = Directory.GetFiles("sql/migrations/");
        foreach (string filePath in files)
        {
            string file = File.ReadAllText(filePath);
            db.Query(file);
        }
        return files.Length;
    }

    internal void RunInitialSql()
    {
        while(TestConnection() == false)
        {
            // Wait for the db to be ready
            System.Threading.Thread.Sleep(1000);
        }
        string initialSql = File.ReadAllText("sql/initialize.sql");
        db.Query(initialSql);
    }
}