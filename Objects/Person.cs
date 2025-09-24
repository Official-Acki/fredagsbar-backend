using Dapper;
using Npgsql;

public class Person : IDbModel<Person>
{
    public int id { get; set; }
    public string? username { get; set; }
    public long? discord_id { get; set; }
    public DateTime? created_at { get; set; }
    public Person(int id, string username, long discord_id, DateTime created_at)
    {
        this.id = id;
        this.username = username;
        this.discord_id = discord_id;
        this.created_at = created_at;
    }

    // Create

    /// <summary>
    /// Creates a new Person object.
    /// </summary>
    /// <param name="args">Expected arguments: (string username, string password)</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<Person?> CreateObj(params object[] args)
    {
        if (args.Length != 2 || args[0] is not string username || args[1] is not string password)
        {
            throw new ArgumentException("Invalid arguments for CreateObj. Expected (string username, string password).");
        }
        string password_hash = BCrypt.Net.BCrypt.HashPassword(password);
        int id = await DatabaseController.Instance.db.ExecuteScalarAsync<int>("INSERT INTO persons (username, password_hash) VALUES (@username, @password_hash) RETURNING id", new { username = username, password_hash = password_hash });
        return await ReadObj(id);
    }

    // Read
    public static async Task<Person?> ReadObj(int id)
    {
        return (await DatabaseController.Instance.db.QueryAsync<Person>("SELECT id, username, discord_id, created_at from persons WHERE id = @id", new { id = id })).FirstOrDefault();
    }

    public static async Task<IEnumerable<Person>> GetAll()
    {
        return await DatabaseController.Instance.db.QueryAsync<Person>("SELECT id, username, discord_id, created_at from persons");
    }

    public static async Task<Person?> ReadObj(long discord_id)
    {
        return (await DatabaseController.Instance.db.QueryAsync<Person>("SELECT id, username, discord_id, created_at from persons WHERE discord_id = @discord_id", new { discord_id = discord_id })).FirstOrDefault();
    }

    public static async Task<Person?> ReadObj(string username)
    {
        return (await DatabaseController.Instance.db.QueryAsync<Person>("SELECT id, username, discord_id, created_at from persons WHERE username = @username", new { username = username })).FirstOrDefault();
    }

    public static async Task<Person?> ReadObj(Guid session_token)
    {
        return (await DatabaseController.Instance.db.QueryAsync<Person>("SELECT p.id, p.username, p.discord_id, p.created_at FROM persons p JOIN user_sessions us ON p.id = us.person_id WHERE us.session_token = @session_token AND us.expires_at > @now", new { session_token = session_token, now = DateTime.UtcNow })).FirstOrDefault();
    }

    public static async Task<string?> GetPasswordHash(int id)
    {
        return (await DatabaseController.Instance.db.QueryAsync<string>("SELECT password_hash FROM persons WHERE id = @id", new { id = id })).FirstOrDefault();
    }


    // Beers
    public static async Task<int> GetTotalBeers()
    {
        return await DatabaseController.Instance.db.QuerySingleAsync<int>("SELECT COUNT(*) FROM beers_drank");
    }

    public static async Task<int> GetTotalBeersByPerson(int person_id)
    {
        return await DatabaseController.Instance.db.QuerySingleAsync<int>("SELECT COUNT(*) FROM beers_drank WHERE person_id = @person_id", new { person_id = person_id });
    }

    public static async Task<int> GetTotalBeers(DateTime date)
    {
        return await DatabaseController.Instance.db.QuerySingleAsync<int>("SELECT COUNT(*) FROM beers_drank WHERE DATE(drank_at) = DATE(@date)", new { date = date });
    }

    public static async Task<int> GetTotalBeersByPerson(int person_id, DateTime date)
    {
        return await DatabaseController.Instance.db.QuerySingleAsync<int>("SELECT COUNT(*) FROM beers_drank WHERE person_id = @person_id AND DATE(drank_at) = DATE(@date)", new { person_id = person_id, date = date });
    }

    public static async Task<int> GetTotalBeersToday()
    {
        return await GetTotalBeers(DateTime.UtcNow);
    }

    public static async Task<int> GetTotalBeersTodayByPerson(int person_id)
    {
        return await GetTotalBeersByPerson(person_id, DateTime.UtcNow);
    }

    public async Task<int> AddBeer()
    {
        return await DatabaseController.Instance.db.ExecuteAsync("INSERT INTO beers_drank (person_id) VALUES (@person_id)", new { person_id = this.id });
    }

}