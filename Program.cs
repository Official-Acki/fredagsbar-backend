using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Tells swagger gen that a nullable int is optional
    c.MapType<int?>(() => new OpenApiSchema { Type = "integer", Nullable = true });
});
builder.Services.AddControllers();
builder.Services.AddSignalR();
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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
    //    app.MapOpenApi(); // Removed as it is not a valid method for WebApplication (was from dotnet 9)
}

app.UseCors();

app.UseHttpsRedirection();

app.MapGet("/ping", () => "pong").WithName("Ping");

app.MapControllers();

// controller=Home, action=Index, id is optional (basically default controller is HomeController and default action inside controllers is Index)
app.MapControllerRoute("default", pattern: "{controller=Home}/{action=Index}/{id?}");

// Web sockets
app.MapHub<LeaderboardHub>("/leaderboardHub");


app.Logger.LogInformation("Starting with invite code: " + Environment.GetEnvironmentVariable("INVITE_CODE"));

app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
