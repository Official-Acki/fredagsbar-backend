using Dapper;

public class EventObject : IDbModel<EventObject>
{

    public class EventTimes
    {
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }
        public TimeSpan repeat_interval { get; set; }
        public EventTimes(DateTime start_time, DateTime end_time, TimeSpan repeat_interval)
        {
            this.start_time = start_time;
            this.end_time = end_time;
            this.repeat_interval = repeat_interval;
        }
    }

    public int? id { get; set; }
    public string? name { get; set; }
    public string? description { get; set; }
    public EventTimes[]? event_times { get; set; }

    public EventObject(int? id, string? name, string? description)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.event_times = event_times;
    }
    public EventObject(int? id, string? name, string? description, EventTimes[]? event_times)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.event_times = event_times;
    }

    public static async Task<EventObject?> CreateObj(params object[] args)
    {
        throw new NotImplementedException();
    }

    public async void CreateObj()
    {
        DatabaseController.Instance.db.Execute("INSERT INTO events (name, description) VALUES (@name, @description)", new { name = this.name, description = this.description });
        for (int i = 0; i < this.event_times?.Length; i++)
        {
            DatabaseController.Instance.db.Execute(@"
                INSERT INTO event_times (event_id, start_time, end_time, repeat_interval)
                VALUES ((SELECT id FROM events WHERE name = @name), @start_time, @end_time, @repeat_interval)",
                new
                {
                    name = this.name,
                    start_time = this.event_times[i].start_time,
                    end_time = this.event_times[i].end_time,
                    repeat_interval = this.event_times[i].repeat_interval
                });
        }
    }

    public static async Task<IEnumerable<EventObject>> GetAll()
    {
        EventObject[] eventObjects = DatabaseController.Instance.db.Query<EventObject>("SELECT id, name, description FROM events").ToArray();
        foreach (EventObject eventObject in eventObjects)
        {
            eventObject.event_times = DatabaseController.Instance.db.Query<EventTimes>("SELECT start_time, end_time, repeat_interval FROM event_times WHERE event_id = @event_id", new { event_id = eventObject.id }).ToArray();
        }
        return eventObjects;
    }

    public static async Task<EventObject?> ReadObj(int id)
    {
        var eventObject = DatabaseController.Instance.db.QueryFirstOrDefault<EventObject>("SELECT id, name, description FROM events WHERE id = @id", new { id = id });
        if (eventObject != null)
        {
            eventObject.event_times = DatabaseController.Instance.db.Query<EventTimes>("SELECT start_time, end_time, repeat_interval FROM event_times WHERE event_id = @event_id", new { event_id = eventObject.id }).ToArray();
        }
        return eventObject;
    }

    private static async Task<EventObject?> ReadObj(string name)
    {
        var eventObject = DatabaseController.Instance.db.QueryFirstOrDefault<EventObject>("SELECT id, name, description FROM events WHERE name = @name", new { name = name });
        if (eventObject != null)
        {
            eventObject.event_times = DatabaseController.Instance.db.Query<EventTimes>("SELECT start_time, end_time, repeat_interval FROM event_times WHERE event_id = @event_id", new { event_id = eventObject.id }).ToArray();
        }
        return eventObject;
    }

    public static async Task<IEnumerable<EventObject>> ReadObj(DateTime time)
    {
        var eventObjects = DatabaseController.Instance.db.Query<EventObject>(@"
            SELECT DISTINCT e.id, e.name, e.description
            FROM events e
            JOIN event_times et ON e.id = et.event_id
            WHERE et.start_time <= @time AND (et.end_time IS NULL OR et.end_time >= @time)", new { time }).ToArray();

        foreach (var eventObject in eventObjects)
        {
            eventObject.event_times = DatabaseController.Instance.db.Query<EventTimes>("SELECT start_time, end_time, repeat_interval FROM event_times WHERE event_id = @event_id", new { event_id = eventObject.id }).ToArray();
        }

        return eventObjects;
    }

    public static async Task<IEnumerable<EventObject>> GetCurrentEvents()
    {
        DateTime now = DateTime.UtcNow;
        return await ReadObj(now);
    }
}