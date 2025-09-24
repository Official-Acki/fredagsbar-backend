using Dapper;

public class CasesOwed : IDbModel<CasesOwed>
{
    public int person_id { get; }
    public float cases { get; }
    public DateTime updated_at { get; }

    public static async Task<CasesOwed?> CreateObj(params object[] args)
    {
        if (args.Length != 2 || args[0] is not int person_id || args[1] is not float cases)
        {
            throw new ArgumentException("Invalid arguments for CreateObj. Expected (int person_id, float cases).");
        }
        await DatabaseController.Instance.db.ExecuteAsync("INSERT INTO cases_owed (person_id, cases) VALUES (@person_id, @cases) ON CONFLICT (person_id) DO UPDATE SET cases = EXCLUDED.cases, updated_at = CURRENT_TIMESTAMP", new { person_id = person_id, cases = cases });
        return await ReadObj(person_id);
    }

    public static async Task<CasesOwed?> ReadObj(int person_id)
    {
        return (await DatabaseController.Instance.db.QueryAsync<CasesOwed>("SELECT person_id, cases, updated_at from cases_owed WHERE person_id = @person_id", new { person_id = person_id })).FirstOrDefault();
    }

    public static async Task<IEnumerable<CasesOwed>> GetAll()
    {
        return (await DatabaseController.Instance.db.QueryAsync<CasesOwed>("SELECT person_id, cases, updated_at from cases_owed")).AsList();
    }

    public static async Task<float> GetTotalOwedCases()
    {
        return await DatabaseController.Instance.db.QuerySingleAsync<float>("SELECT SUM(cases_owed.cases)+SUM(cases_given.cases) FROM cases_owed LEFT JOIN cases_given ON cases_owed.person_id = cases_given.person_id");
    }

    public static async Task<float> GetTotalOwedCases(int person_id)
    {
        return await DatabaseController.Instance.db.QuerySingleAsync<float>(
            "SELECT COALESCE(SUM(cases_owed.cases), 0) + COALESCE(SUM(cases_given.cases), 0) FROM cases_owed LEFT JOIN cases_given ON cases_owed.person_id = cases_given.person_id WHERE cases_owed.person_id = @person_id",
            new { person_id = person_id }
        );
    }

    public static async Task<float> UpdateOwedCases(int person_id, float cases)
    {
        await DatabaseController.Instance.db.ExecuteAsync(
            "INSERT INTO cases_owed (person_id, cases) VALUES (@person_id, @cases) ON CONFLICT (person_id) DO UPDATE SET cases = EXCLUDED.cases, updated_at = CURRENT_TIMESTAMP",
            new { person_id = person_id, cases = cases }
        );
        return cases;
    }
}

public class CasesGiven : IDbModel<CasesGiven>
{
    public int person_id { get; }
    public DateTime given_at { get; }
    public float cases { get; }

    public static async Task<CasesGiven?> CreateObj(params object[] args)
    {
        if (args.Length != 2 || args[0] is not int person_id || args[1] is not float cases)
        {
            throw new ArgumentException("Invalid arguments for CreateObj. Expected (int person_id, float cases).");
        }
        await DatabaseController.Instance.db.ExecuteAsync("INSERT INTO cases_given (person_id, cases) VALUES (@person_id, @cases)", new { person_id = person_id, cases = cases });
        return await ReadObj(person_id);
    }

    public static async Task<CasesGiven?> ReadObj(int id)
    {
        return (await DatabaseController.Instance.db.QueryAsync<CasesGiven>("SELECT person_id, given_at, cases from cases_given WHERE person_id = @person_id", new { person_id = id })).FirstOrDefault();
    }

    public static async Task<IEnumerable<CasesGiven>> GetAll()
    {
        return (await DatabaseController.Instance.db.QueryAsync<CasesGiven>("SELECT person_id, given_at, cases from cases_given")).AsList();
    }

    public static async Task<List<CasesGiven>> GetAll(int person_id)
    {
        return (await DatabaseController.Instance.db.QueryAsync<CasesGiven>(
            "SELECT person_id, given_at, cases FROM cases_given WHERE person_id = @person_id",
            new { person_id = person_id }
        )).ToList();
    }

    public static async Task<float> GetTotalGivenCases()
    {
        return await DatabaseController.Instance.db.QuerySingleAsync<float>("SELECT COALESCE(SUM(cases), 0) FROM cases_given");
    }

    public static async Task<float> GetTotalGivenCases(int person_id)
    {
        return await DatabaseController.Instance.db.QuerySingleAsync<float>(
            "SELECT COALESCE(SUM(cases), 0) FROM cases_given WHERE person_id = @person_id",
            new { person_id = person_id }
        );
    }
}