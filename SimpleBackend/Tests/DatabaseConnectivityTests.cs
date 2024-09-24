using Xunit;                       // xUnit testing framework
using Microsoft.EntityFrameworkCore;  // Entity Framework Core
using DotNetEnv;                    // DotNetEnv for loading .env file
using Amazon.SecretsManager;        // AWS Secrets Manager SDK
using Amazon.SecretsManager.Model;  // AWS Secrets Manager SDK
using System.Text.Json;             // System.Text.Json for JSON handling
using System.Threading.Tasks;       // Asynchronous Task handling
using System.Collections.Generic;   // For dictionary
using SimpleBackend.Data;

namespace tests
{
    public class DatabaseConnectivityTests
    {
        private readonly string _secretName;
        private readonly string _region;

        public DatabaseConnectivityTests()
        {
            // Load environment variables from the .env file
            Env.Load();
            
            // Fetch Secret Name and AWS Region from .env
            _secretName = Env.GetString("AWS_SECRET_NAME") ?? throw new ArgumentNullException("AWS_SECRET_NAME");
            _region = Env.GetString("AWS_REGION") ?? throw new ArgumentNullException("AWS_REGION");
        }

        private async Task<DbContextOptions<SimpleDbContext>> GetDatabaseOptions()
        {
            // Fetch database credentials from AWS Secrets Manager
            var dbCredentials = await GetSecret(_secretName, _region);

            // Debugging to check if credentials are null
            Console.WriteLine($"Fetched DB credentials: Host={dbCredentials?["DB_HOST"]}, User={dbCredentials?["DB_USER"]}");

            if (dbCredentials == null || dbCredentials.Count == 0)
            {
                throw new Exception("No database credentials found.");
            }

            var options = new DbContextOptionsBuilder<SimpleDbContext>()
                .UseSqlServer($"Server={dbCredentials["DB_HOST"]},{dbCredentials["DB_PORT"]};Database={dbCredentials["DB_NAME"]};User Id={dbCredentials["DB_USER"]};Password={dbCredentials["DB_PASSWORD"]};TrustServerCertificate=True;")
                .Options;

            return options;
        }

        [Fact]
        public async Task CanConnectToDatabase()
        {
            try
            {
                // Get DbContext options with credentials from Secrets Manager
                var options = await GetDatabaseOptions();
                
                // Debugging print to ensure credentials are not null
                Console.WriteLine($"DB_HOST: {options}");
                
                using (var dbContext = new SimpleDbContext(options))
                {
                    // Test the database connection
                    bool canConnect = dbContext.Database.CanConnect();
                    Assert.True(canConnect, "Unable to connect to the database.");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception occurred while connecting to the database: {ex.Message}");
            }
        }

        // Method to retrieve secret values from AWS Secrets Manager
        private async Task<Dictionary<string, string>?> GetSecret(string secretName, string region)
        {
            // Create a client to interact with AWS Secrets Manager
            IAmazonSecretsManager client = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.GetBySystemName(region));

            // Create the request to get the secret value
            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
            };

            GetSecretValueResponse response;

            try
            {
                // Fetch the secret value
                response = await client.GetSecretValueAsync(request);
            }
            catch (Exception e)
            {
                throw new Exception($"Error fetching secret: {e.Message}");
            }

            // Parse the secret as a JSON string
            string secret = response.SecretString;
            if (string.IsNullOrEmpty(secret))
            {
                throw new Exception("No secret value found.");
            }

            // Deserialize the secret JSON into a dictionary and return it
            return JsonSerializer.Deserialize<Dictionary<string, string>>(secret)!;
        }
    }
}
