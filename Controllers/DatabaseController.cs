using Npgsql;
using BCrypt.Net;

public class DatabaseController
{
    // Singleton instance with only a get method, instanciating itself.
    public static DatabaseController Instance { get; } = new DatabaseController();
    private string connectionString;

    public DatabaseController()
    {
        this.connectionString = "Host=" + Environment.GetEnvironmentVariable("DB_HOST") + ":" + Environment.GetEnvironmentVariable("DB_PORT") +
            ";Username=" + Environment.GetEnvironmentVariable("DB_USER") +
            ";Password=" + Environment.GetEnvironmentVariable("DB_PASSWORD") +
            ";Database=" + Environment.GetEnvironmentVariable("DB_NAME");
    }

    private NpgsqlDataReader? ExecuteCommand(NpgsqlCommand cmd)
    {
        NpgsqlDataReader? reader;
        try { reader = cmd.ExecuteReader(); }
        catch (Exception ex)
        {
            Console.WriteLine("Error executing query: " + ex.Message + "\n" + ex.StackTrace);
            return null; // Query failed
        }
        return reader;
    }

    #region Session Handling

    public bool VerifySession(Guid sessionToken)
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT id FROM user_sessions WHERE session_token = @session_token AND expires_at > @now", conn);
        cmd.Parameters.AddWithValue("session_token", sessionToken);
        cmd.Parameters.AddWithValue("now", DateTime.UtcNow);

        using var reader = ExecuteCommand(cmd);
        if (reader == null) return false; // Query failed
        bool isValid = reader.Read(); // If there's a row, the session is valid
        conn.CloseAsync();
        return isValid;
    }



    /// <summary>
    /// Only call this when sure the person is verified.
    /// </summary>
    /// <param name="person"></param>
    /// <returns>Session? (only null if something went wrong)</returns>
    public Session? CreateSession(Person person)
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("INSERT INTO user_sessions (person_id, expires_at) VALUES (@person_id, @expires_at) RETURNING id, person_id, session_token, created_at, expires_at", conn);
        cmd.Parameters.AddWithValue("person_id", person.id);
        cmd.Parameters.AddWithValue("expires_at", DateTime.UtcNow.AddDays(7)); // Sessions last 7 days

        using var reader = ExecuteCommand(cmd);
        if (reader == null) return null; // Query failed
        if (reader.Read())
        {
            int sessionId = reader.GetInt32(0);
            int userId = reader.GetInt32(1);
            Guid sessionToken = reader.GetGuid(2);
            DateTime createdAt = reader.GetDateTime(3);
            DateTime expiresAt = reader.GetDateTime(4);

            conn.CloseAsync();

            this.CloseOldSessions();

            return new Session(sessionId, userId, sessionToken, createdAt, expiresAt);
        }
        else
        {
            return null; // Insertion failed
        }
    }

    private void CloseOldSessions()
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("DELETE FROM user_sessions WHERE expires_at < @now", conn);
        cmd.Parameters.AddWithValue("now", DateTime.UtcNow);

        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error deleting old sessions: " + ex.Message + "\n" + ex.StackTrace);
        }
        finally
        {
            conn.CloseAsync();
        }
    }




    #endregion




    // Method to get a person by id
    public Person? GetPerson(int id)
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT id, username, discord_id, created_at FROM persons WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        using var reader = ExecuteCommand(cmd);
        if (reader == null) return null; // Query failed
        if (reader.Read())
        {
            int personId = reader.GetInt32(0);
            string username = reader.GetString(1);
            UInt64 discordId = (UInt64)reader.GetInt64(2);
            DateTime createdAt = reader.GetDateTime(3);

            conn.CloseAsync();
            return new Person(personId, username, discordId, createdAt);
        }
        else
        {
            conn.CloseAsync();
            return null; // No person found with the given id
        }
    }

    public Person? GetPerson(string username)
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT id, username, discord_id, created_at FROM persons WHERE username = @username", conn);
        cmd.Parameters.AddWithValue("username", username);
        using var reader = ExecuteCommand(cmd);
        if (reader == null) return null; // Query failed
        if (reader.Read())
        {
            int id = reader.GetInt32(0);
            string personUsername = reader.GetString(1);
            UInt64 discordId = (UInt64)reader.GetInt64(2);
            DateTime createdAt = reader.GetDateTime(3);
            conn.CloseAsync();
            return new Person(id, personUsername, discordId, createdAt);
        }
        else
        {
            conn.CloseAsync();
            return null; // No person found with the given id
        }
    }

    public string? GetPasswordHash(Person person)
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT password_hash FROM persons WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", person.id);
        using var reader = ExecuteCommand(cmd);
        if (reader == null) return null; // Query failed
        if (reader.Read())
        {
            string password_hash = reader.GetString(0);
            conn.CloseAsync();
            return password_hash;
        }
        else
        {
            conn.CloseAsync();
            return null; // No person found with the given id
        }
    }

    // Method to register a person
    public Person? CreatePerson(string username, UInt64 discord_id, string password)
    {
        // Hash the password before storing it
        password = BCrypt.Net.BCrypt.HashPassword(password);


        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("INSERT INTO persons (username, discord_id, password_hash) VALUES (@username, @discord_id, @password_hash) RETURNING id, created_at", conn);
        cmd.Parameters.AddWithValue("username", username);
        cmd.Parameters.AddWithValue("discord_id", (long)discord_id); // Cast to long for PostgreSQL compatibility
        cmd.Parameters.AddWithValue("password_hash", password);

        using var reader = ExecuteCommand(cmd);
        if (reader == null) return null; // Query failed
        if (reader.Read())
        {
            int personId = reader.GetInt32(0);
            DateTime createdAt = reader.GetDateTime(1);

            conn.CloseAsync();
            return new Person(personId, username, discord_id, createdAt);
        }
        else
        {
            conn.CloseAsync();
            return null; // Insertion failed
        }
    }

}