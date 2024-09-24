using Amazon.CloudWatchLogs;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.XRay.Recorder.Handlers.AspNetCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SimpleBackend.Data;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
Env.Load();

// Retrieve environment variables for Secret Manager usage
string secretName = Env.GetString("AWS_SECRET_NAME")!;
string region = Env.GetString("AWS_REGION")!;

// Fetch RDS credentials from Secrets Manager
var rdsCredentials = await Helpers.GetSecret(secretName, region);

// Print the credentials to the console (for testing purposes)
Console.WriteLine($"Fetched credentials from Secrets Manager:");
Console.WriteLine($"Host: {rdsCredentials!["bucket"]}");
Console.WriteLine($"Port: {rdsCredentials["password"]}");
Console.WriteLine($"Username: {rdsCredentials["organization"]}");
Console.WriteLine($"Password: {rdsCredentials["username"]}");

// Configure DbContext to use SQL Server with the loaded environment variables
builder.Services.AddDbContext<SimpleDbContext>(options =>
    options.UseSqlServer($"Data Source={rdsCredentials["host"]},{rdsCredentials["port"]};Initial Catalog={rdsCredentials["dbname"]};User ID={rdsCredentials["username"]};Password={rdsCredentials["password"]};TrustServerCertificate=True;"));

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

// Add Swagger service but only use it in development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen();
}

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

// Enable Swagger only in development environment
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
