using Npgsql;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;

public class DatabaseController
{
    // Singleton instance with only a get method, instanciating itself.
    public static DatabaseController Instance { get; } = new DatabaseController();
    public static float SESSION_VALIDITY_DAYS = Environment.GetEnvironmentVariable("SESSION_VALIDITY_DAYS") != null ? float.Parse(Environment.GetEnvironmentVariable("SESSION_VALIDITY_DAYS")!) : 7;
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

    public bool VerifySession(Guid sessionToken, int person_id)
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT id FROM user_sessions WHERE session_token = @session_token AND expires_at > @now AND person_id = @person_id", conn);
        cmd.Parameters.AddWithValue("session_token", sessionToken);
        cmd.Parameters.AddWithValue("now", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("person_id", person_id);

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
        cmd.Parameters.AddWithValue("expires_at", DateTime.UtcNow.AddDays(SESSION_VALIDITY_DAYS)); // Sessions last 7 days

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

    public Session? RenewSession(Guid session_token)
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("UPDATE user_sessions SET expires_at = @new_expires_at WHERE session_token = @session_token RETURNING id, person_id, session_token, created_at, expires_at", conn);
        cmd.Parameters.AddWithValue("new_expires_at", DateTime.UtcNow.AddDays(SESSION_VALIDITY_DAYS));
        cmd.Parameters.AddWithValue("session_token", session_token);
        using var reader = ExecuteCommand(cmd);
        if (reader == null) return null; // Query failed
        if (reader.Read())
        {
            int sessionId = reader.GetInt32(0);
            int userId = reader.GetInt32(1);
            Guid newSessionToken = reader.GetGuid(2);
            DateTime createdAt = reader.GetDateTime(3);
            DateTime newExpiresAt = reader.GetDateTime(4);

            conn.CloseAsync();
            this.CloseOldSessions();

            return new Session(sessionId, userId, newSessionToken, createdAt, newExpiresAt);
        }
        else
        {
            conn.CloseAsync();
            return null; // Update failed
        }
    }

    public bool DeleteSession(Guid session_token)
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("DELETE FROM user_sessions WHERE session_token = @guid", conn);
        cmd.Parameters.AddWithValue("guid", session_token);
        bool result = false;
        try
        {
            result = cmd.ExecuteNonQuery() > -1 ? true : false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error deleting session: " + ex.Message + "\n" + ex.StackTrace);
        }
        finally
        {
            conn.CloseAsync();
        }
        return result;
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
    #region Person Handling



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
            UInt64 discordId = !reader.IsDBNull(2) ? reader.GetFieldValue<UInt64>(2) : 0;
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
            UInt64 discordId = !reader.IsDBNull(2) ? reader.GetFieldValue<UInt64>(2) : 0;
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

    public Person? GetPerson(Guid session_token)
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT p.id, p.username, p.discord_id, p.created_at FROM persons p JOIN user_sessions us ON p.id = us.person_id WHERE us.session_token = @session_token AND us.expires_at > @now", conn);
        cmd.Parameters.AddWithValue("session_token", session_token);
        cmd.Parameters.AddWithValue("now", DateTime.UtcNow);
        using var reader = ExecuteCommand(cmd);
        if (reader == null) return null; // Query failed
        if (reader.Read())
        {
            int id = reader.GetInt32(0);
            string username = reader.GetString(1);
            UInt64 discordId = !reader.IsDBNull(2) ? reader.GetFieldValue<UInt64>(2) : 0;
            DateTime createdAt = reader.GetDateTime(3);
            conn.CloseAsync();
            return new Person(id, username, discordId, createdAt);
        }
        else
        {
            conn.CloseAsync();
            return null; // No person found with the given session token
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
    public Person? CreatePerson(string username, string password)
    {
        // Hash the password before storing it
        password = BCrypt.Net.BCrypt.HashPassword(password);


        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("INSERT INTO persons (username, password_hash) VALUES (@username, @password_hash) RETURNING id, created_at", conn);
        cmd.Parameters.AddWithValue("username", username);
        // cmd.Parameters.AddWithValue("discord_id", (long)discord_id); // Cast to long for PostgreSQL compatibility
        cmd.Parameters.AddWithValue("password_hash", password);

        using var reader = ExecuteCommand(cmd);
        if (reader == null) return null; // Query failed
        if (reader.Read())
        {
            int personId = reader.GetInt32(0);
            DateTime createdAt = reader.GetDateTime(1);

            conn.CloseAsync();
            return new Person(personId, username, 0, createdAt);
        }
        else
        {
            conn.CloseAsync();
            return null; // Insertion failed
        }
    }

    #endregion

    #region Beer Handling

    public int GetTotalBeers()
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM beers_drank", conn);
        using var reader = ExecuteCommand(cmd);
        if (reader == null) return 0; // Query failed
        if (reader.Read())
        {
            int totalBeers = reader.GetInt32(0);
            conn.CloseAsync();
            return totalBeers;
        }
        else
        {
            conn.CloseAsync();
            return 0; // No beers found
        }
    }

    public int GetTotalBeersByPerson(int person_id)
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM beers_drank WHERE person_id = @person_id", conn);
        cmd.Parameters.AddWithValue("person_id", person_id);
        using var reader = ExecuteCommand(cmd);
        if (reader == null) return 0; // Query failed
        if (reader.Read())
        {
            int totalBeers = reader.GetInt32(0);
            conn.CloseAsync();
            return totalBeers;
        }
        else
        {
            conn.CloseAsync();
            return 0; // No beers found
        }
    }

    public int GetTotalBeers(DateTime date)
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM beers_drank WHERE DATE(drank_at) = DATE(@date)", conn);
        cmd.Parameters.AddWithValue("date", date.ToUniversalTime().Date);
        using var reader = ExecuteCommand(cmd);
        if (reader == null) return 0; // Query failed
        if (reader.Read())
        {
            int totalBeers = reader.GetInt32(0);
            conn.CloseAsync();
            return totalBeers;
        }
        else
        {
            conn.CloseAsync();
            return 0; // No beers found
        }
    }

    public int GetTotalBeersByPerson(DateTime date, int person_id)
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM beers_drank WHERE person_id = @person_id AND DATE(drank_at) = DATE(@date)", conn);
        cmd.Parameters.AddWithValue("person_id", person_id);
        cmd.Parameters.AddWithValue("date", date.ToUniversalTime().Date);
        using var reader = ExecuteCommand(cmd);
        if (reader == null) return 0; // Query failed
        if (reader.Read())
        {
            int totalBeers = reader.GetInt32(0);
            conn.CloseAsync();
            return totalBeers;
        }
        else
        {
            conn.CloseAsync();
            return 0; // No beers found
        }
    }

    public int GetTotalBeersToday()
    {
        return GetTotalBeers(DateTime.UtcNow);
    }

    public int GetTotalBeersTodayByPerson(int person_id)
    {
        return GetTotalBeersByPerson(DateTime.UtcNow, person_id);
    }

    public bool AddBeerToPerson(int person_id)
    {
        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("INSERT INTO beers_drank (person_id) VALUES (@person_id)", conn);
        cmd.Parameters.AddWithValue("person_id", person_id);
        try
        {
            int rowsAffected = cmd.ExecuteNonQuery();
            conn.CloseAsync();
            return rowsAffected > 0; // Return true if at least one row was inserted
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error adding beer: " + ex.Message + "\n" + ex.StackTrace);
            conn.CloseAsync();
            return false; // Insertion failed
        }
    }

    #endregion

    #region Leaderboard

    public class LeaderboardEntry
    {
        public Person? person { get; set; }
        public int drank { get; set; }
    }

    public List<LeaderboardEntry> GetLeaderboardEntries(int amount = 5)
    {
        List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(@"
            SELECT persons.id, persons.username, COUNT(beers_drank) AS beers_drank 
            FROM persons
            LEFT JOIN beers_drank ON persons.id = beers_drank.person_id
            GROUP BY persons.id
            ORDER BY beers_drank DESC
            LIMIT @amount
        ", conn);
        cmd.Parameters.AddWithValue("amount", amount);
        using var reader = ExecuteCommand(cmd);
        if (reader == null) return entries; // Query failed
        while (reader.Read())
        {
            int personId = reader.GetInt32(0);
            string username = reader.GetString(1);
            int beersDrank = reader.GetInt32(2);

            entries.Add(new LeaderboardEntry
            {
                person = new Person(personId, username, 0, DateTime.MinValue), // Discord ID and created_at are not needed here
                drank = beersDrank
            });
        }
        conn.CloseAsync();
        return entries;
    }

    public List<LeaderboardEntry> GetLeaderboardEntriesByDate(DateTime date, int amount = 5)
    {
        List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

        using var conn = new NpgsqlConnection(this.connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand(@"
            SELECT persons.id, persons.username, COUNT(beers_drank) AS beers_drank 
            FROM persons
            LEFT JOIN beers_drank ON persons.id = beers_drank.person_id
            WHERE DATE(beers_drank.drank_at) = DATE(@date)
            GROUP BY persons.id
            ORDER BY beers_drank DESC
            LIMIT @amount
        ", conn);
        cmd.Parameters.AddWithValue("amount", amount);
        cmd.Parameters.AddWithValue("date", date.ToUniversalTime().Date);
        using var reader = ExecuteCommand(cmd);
        if (reader == null) return entries; // Query failed
        while (reader.Read())
        {
            int personId = reader.GetInt32(0);
            string username = reader.GetString(1);
            int beersDrank = reader.GetInt32(2);

            entries.Add(new LeaderboardEntry
            {
                person = new Person(personId, username, 0, DateTime.MinValue), // Discord ID and created_at are not needed here
                drank = beersDrank
            });
        }
        conn.CloseAsync();
        return entries;
    }

    public List<LeaderboardEntry> GetLeaderboardEntriesToday(int amount = 5)
    {
        return GetLeaderboardEntriesByDate(DateTime.UtcNow, amount);
    }

    #endregion
}