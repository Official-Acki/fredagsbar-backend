using Dapper;

public class Leaderboard
{
    public class LeaderboardEntry
    {
        public Person? person { get; set; }
        public int beers_drank { get; set; }
    }

    public List<LeaderboardEntry> entries { get; set; }

    public Leaderboard(List<LeaderboardEntry> entries)
    {
        this.entries = entries;
    }

    public static async Task<Leaderboard?> BeersDrankLeaderboard(int amount = 20)
    {
        var entries = (await DatabaseController.Instance.db.QueryAsync<Person, long, LeaderboardEntry>(
            "SELECT persons.id, persons.username, persons.discord_id, persons.created_at, COUNT(beers_drank) AS beers_drank " +
            "FROM persons " +
            "LEFT JOIN beers_drank ON persons.id = beers_drank.person_id " +
            "GROUP BY persons.id " +
            "ORDER BY beers_drank DESC " +
            "LIMIT @amount",
            (person, beersDrank) => new LeaderboardEntry { person = person, beers_drank = (int)beersDrank },
            new { amount },
            splitOn: "beers_drank"
        )).ToList();

        return new Leaderboard(entries);
    }
    
    public static async Task<Leaderboard?> BeersDrankLeaderboard(DateTime date, int amount = 20)
    {
        var entries = (await DatabaseController.Instance.db.QueryAsync<Person, long, DateTime?, LeaderboardEntry>(
            "SELECT p.id, p.username, p.discord_id, p.created_at, " +
            "COUNT(bd.*) AS beers_drank, MAX(bd.drank_at) AS last_drank_at " +
            "FROM persons p " +
            "LEFT JOIN beers_drank bd ON p.id = bd.person_id " +
            "WHERE DATE(bd.drank_at) = DATE(@date) " +
            "GROUP BY p.id " +
            "ORDER BY beers_drank DESC, last_drank_at ASC " +
            "LIMIT @amount",
            (person, beersDrank, lastDrankAt) => new LeaderboardEntry {
                person = person,
                beers_drank = (int)beersDrank,
            },
            new { amount, date = date.Date },
            splitOn: "beers_drank,last_drank_at"
        )).ToList();

        return new Leaderboard(entries);
    }

}