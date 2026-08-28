using Fredagsbar.Backend.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

const bool IN_MEMORY_DB = false;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

// Singletons
builder.Services.AddSingleton<InternalOnlyFilter>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(
		policy =>
		{
			policy
				.SetIsOriginAllowed(_ => true)
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials();
		});
});
builder.Services.AddDbContextPool<ApplicationDbContext>(opt =>
{
	if (builder.Environment.IsDevelopment())
	{
		if (IN_MEMORY_DB)
		{
			opt.UseInMemoryDatabase("LocalDev");
		}
		else
		{
			opt.UseSqlite("Data Source=LocalDev.db");
		}
	}
	else
	{
		opt.UseNpgsql(
			new NpgsqlConnectionStringBuilder
			{
				Host = config["DB:Host"] ?? throw new InvalidOperationException("DB:Host not configured"),
				Port = Convert.ToInt32(config["DB:Port"] ?? throw new InvalidOperationException("DB:Port not configured")),
				Username = config["DB:User"] ?? throw new InvalidOperationException("DB:User not configured"),
				Password = config["DB:Password"] ?? throw new InvalidOperationException("DB:Password not configured"),
				Database = config["DB:Name"] ?? throw new InvalidOperationException("DB:Name not configured"),
			}.ConnectionString,
			o => o
				.SetPostgresVersion(18, 0)
		// .MapEnum<Mood>("mood")
		);
	}
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
	//    app.MapOpenApi(); // Removed as it is not a valid method for WebApplication (was from dotnet 9)
}

app.UseCors();

app.UseHttpsRedirection();

app.MapControllers();

// controller=Home, action=Index, id is optional (basically default controller is HomeController and default action inside controllers is Index)
app.MapControllerRoute("default", pattern: "{controller=Home}/{action=Index}/{id?}");

// Web sockets
// app.MapHub<LeaderboardHub>("/leaderboardHub");
if (!IN_MEMORY_DB)
{
	app.Logger.LogInformation("Running migrations (if any)...");
	await app.Services.MigrateAsync(app.Environment);
	app.Logger.LogInformation("Migrations complete");
}
await app.Services.SeedRules();

app.Run();
