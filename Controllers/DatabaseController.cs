using Npgsql;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using System.Data;

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

}