
public class Person
{
    public int id { get; }
    public string username { get; }
    public UInt64 discord_id { get; }
    public DateTime created_at { get; }

    public Person(int id, string username, UInt64 discord_id, DateTime created_at)
    {
        this.id = id;
        this.username = username;
        this.discord_id = discord_id;
        this.created_at = created_at;
    }

    public override string ToString()
    {
        return $"Person(id={id}, username={username}, discord_id={discord_id}, created_at={created_at})";
    }

}