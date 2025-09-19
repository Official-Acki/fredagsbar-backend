public class Session
{
    public int id { get; }
    public int user_id { get; }
    public Guid session_token { get; }
    public DateTime created_at { get; }
    public DateTime expires_at { get; }

    public Session(int id, int user_id, Guid session_token, DateTime created_at, DateTime expires_at)
    {
        this.id = id;
        this.user_id = user_id;
        this.session_token = session_token;
        this.created_at = created_at;
        this.expires_at = expires_at;
    }

    public override string ToString()
    {
        return $"Session(id={id}, user_id={user_id}, session_token={session_token}, created_at={created_at}, expires_at={expires_at})";
    }
}