using Microsoft.EntityFrameworkCore;
using SimpleBackend.Data;
using DotNetEnv;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
Env.Load();

// Retrieve environment variables
string hostname = Env.GetString("DB_HOST");
string port = Env.GetString("DB_PORT");
string database = Env.GetString("DB_NAME");
string username = Env.GetString("DB_USER");
string password = Env.GetString("DB_PASSWORD");

// Configure DbContext to use SQL Server with the loaded environment variables
builder.Services.AddDbContext<SimpleDbContext>(options =>
    options.UseSqlServer($"Data Source={hostname},{port};Initial Catalog={database};User ID={username};Password={password};TrustServerCertificate=True;"));

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        x.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Add root route and connectdb route for testing
app.MapGet("/", () => "Welcome to My API! Navigate to /swagger to explore the API documentation.");
app.MapGet("/connectdb", () => Helpers.TestRDSConnection());

app.Run();

public static class Helpers
{
    public static string TestRDSConnection()
    {
        // Use the same connection string logic to test the database connection
        string connectionString = GetRDSConnectionString();
        using (var connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                return "Successfully connected to the SQL Server on Amazon RDS.";
            }
            catch (Exception ex)
            {
                return $"Connection failed: {ex.Message}";
            }
        }
    }

    public static string GetRDSConnectionString()
    {
        // Load environment variables if not already loaded
        Env.Load();

        // Fetch environment variables for the connection string
        string username = Env.GetString("DB_USER");
        string password = Env.GetString("DB_PASSWORD");
        string hostname = Env.GetString("DB_HOST");
        string port = Env.GetString("DB_PORT");
        string database = Env.GetString("DB_NAME");

        return $"Data Source={hostname},{port};Initial Catalog={database};User ID={username};Password={password};TrustServerCertificate=True;";
    }
}
