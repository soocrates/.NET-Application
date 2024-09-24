using DotNetEnv;
using System.Data.SqlClient;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon;
using System.Text.Json;

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
                await Task.Delay(TimeSpan.FromMinutes(2)); // Keep the connection open for 2 minutes
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

    public static async Task<Dictionary<string, string>> GetSecret(string secretName, string region)
    {
        IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

        GetSecretValueRequest request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
        };

        GetSecretValueResponse response;
        try
        {
            response = await client.GetSecretValueAsync(request);
        }
        catch (Exception e)
        {
            throw e; // Handle exceptions appropriately in production
        }

        string secret = response.SecretString;
        if (string.IsNullOrEmpty(secret))
        {
            throw new Exception("No secret value found.");
        }

        // Parse the secret JSON string into a dictionary
        return JsonSerializer.Deserialize<Dictionary<string, string>>(secret);
    }
}
