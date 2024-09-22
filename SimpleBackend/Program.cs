using Amazon.CloudWatchLogs;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.XRay.Recorder.Handlers.AspNetCore;
using Microsoft.Extensions.Logging;
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

// Add AWS X-Ray and CloudWatch logging services
builder.Services.AddAWSService<IAmazonCloudWatchLogs>();
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddAWSProvider(); // Logs to CloudWatch
    logging.SetMinimumLevel(LogLevel.Information); // Log info and higher levels
});

// Enable X-Ray tracing for all AWS SDK services
AWSSDKHandler.RegisterXRayForAllServices();

// Add controllers and Swagger services
builder.Services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    x.JsonSerializerOptions.WriteIndented = true;
});
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add X-Ray tracing middleware
app.UseXRay("SimpleBackendApp");

// Middleware to log every request and response with status codes
app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    // Log request details
    logger.LogInformation("Request: {method} {url}", context.Request.Method, context.Request.Path);

    // Call the next middleware and capture the response status
    await next.Invoke();

    var statusCode = context.Response.StatusCode;
    logger.LogInformation("Response: Status {statusCode}", statusCode);

    // Add status code to X-Ray trace
    AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", statusCode.ToString());
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Predefined routes for generating different status codes
app.MapGet("/success", () => Results.Ok("This is a 200 response"));
app.MapGet("/notfound", () => Results.NotFound(new { Message = "This is a 404 response" }));
app.MapGet("/servererror", () => Results.Json(new { Message = "This is a 500 response" }, statusCode: 500));
app.MapGet("/redirect", () => Results.Redirect("https://example.com"));
app.MapGet("/connectdb", () => Helpers.TestRDSConnection());

app.Run();

public static class Helpers
{
    public static async Task<string> TestRDSConnection()
    {
        string connectionString = GetRDSConnectionString();
        using (var connection = new SqlConnection(connectionString))
        {
            try
            {
                connection.Open();
                // Keep the connection open for 2 minutes (120,000 milliseconds)
                await Task.Delay(TimeSpan.FromMinutes(2));

                return "Successfully connected to SQL Server on Amazon RDS.";
            }
            catch (Exception ex)
            {
                return $"Connection failed: {ex.Message}";
            }
        }
    }

    public static string GetRDSConnectionString()
    {
        Env.Load();
        string username = Env.GetString("DB_USER");
        string password = Env.GetString("DB_PASSWORD");
        string hostname = Env.GetString("DB_HOST");
        string port = Env.GetString("DB_PORT");
        string database = Env.GetString("DB_NAME");

        return $"Data Source={hostname},{port};Initial Catalog={database};User ID={username};Password={password};TrustServerCertificate=True;";
    }
}
