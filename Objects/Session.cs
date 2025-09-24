using Dapper;

public class Session : IDbModel<Session>
{
    public int? id { get; }
    public int? person_id { get; }
    public Guid? session_token { get; }
    public DateTime? created_at { get; }
    public DateTime? expires_at { get; }

    public Session(int? id, int? person_id, Guid? session_token, DateTime? created_at, DateTime? expires_at)
    {
        this.id = id;
        this.person_id = person_id;
        this.session_token = session_token;
        this.created_at = created_at;
        this.expires_at = expires_at;
    }
    public Session(Guid guid)
    {
        this.session_token = guid;
    }

    public override string ToString()
    {
        return $"Session(id={id}, person_id={person_id}, session_token={session_token}, created_at={created_at}, expires_at={expires_at})";
    }

    public static async Task<Session?> CreateObj(params object[] args)
    {
        if (args.Length != 1 || args[0] is not Person person) return null;
        return await DatabaseController.Instance.db.QueryFirstOrDefaultAsync<Session>(
            "INSERT INTO user_sessions (person_id, expires_at) VALUES (@person_id, @expires_at) RETURNING id, person_id, session_token, created_at, expires_at",
            new { person_id = person.id, expires_at = DateTime.UtcNow.AddDays(DatabaseController.SESSION_VALIDITY_DAYS) }
        );
    }

    public static Task<Session?> ReadObj(int id)
    {
        throw new Exception("Not allowed");
    }

    public static Task<IEnumerable<Session>> GetAll()
    {
        throw new Exception("Not allowed");
    }

    public async Task<bool> VerifySession()
    {
        if (this.session_token == null) return false;
        return await DatabaseController.Instance.db.ExecuteScalarAsync<bool>(
            "SELECT COUNT(*) > 0 FROM user_sessions WHERE session_token = @guid AND expires_at > NOW()",
            new { guid = this.session_token }
        );
    }

    public async Task<bool> VerifySession(int person_id)
    {
        if (this.session_token == null) return false;
        return await DatabaseController.Instance.db.ExecuteScalarAsync<bool>(
            "SELECT COUNT(*) > 0 FROM user_sessions WHERE session_token = @guid AND expires_at > NOW() AND person_id = @person_id",
            new { guid = this.session_token, person_id = person_id }
        );
    }

    public async Task<Session?> Renew()
    {
        if (this.session_token == null) return null;
        // If past expiry date, cannot renew
        if (this.expires_at != null && this.expires_at < DateTime.UtcNow) return null;
        return await DatabaseController.Instance.db.QueryFirstOrDefaultAsync<Session>(
            "UPDATE user_sessions SET expires_at = @new_expires_at WHERE session_token = @session_token RETURNING id, person_id, session_token, created_at, expires_at",
            new { new_expires_at = DateTime.UtcNow.AddDays(DatabaseController.SESSION_VALIDITY_DAYS), session_token = this.session_token }
        );
    }

    // Delete
    public async Task<bool> Delete()
    {
        if (this.session_token == null) return false;
        int rowsAffected = await DatabaseController.Instance.db.ExecuteAsync(
            "DELETE FROM user_sessions WHERE session_token = @session_token",
            new { session_token = this.session_token }
        );
        return rowsAffected > 0;
    }

    public static async Task<int> DeleteOldSessions()
    {
        return await DatabaseController.Instance.db.ExecuteAsync(
            "DELETE FROM user_sessions WHERE expires_at < @now",
            new { now = DateTime.UtcNow }
        );
    }

}