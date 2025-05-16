using System;

namespace AutenticacaoFunction.Configuration
{
    public class AppConfig
    {
        // Database
        public string ConnectionString { get; set; }
        
        // Cognito
        public string UserPoolId { get; set; }
        public string ClientId { get; set; }
        public string AwsAccessKeyId { get; set; }
        public string AwsSecretAccessKey { get; set; }
        public string AwsSessionToken { get; set; }
        public string AwsRegion { get; set; }
        
        // JWT
        public string JwtSecret { get; set; }
        public int JwtExpirationHours { get; set; }
    }

    public static class ConfigurationFactory
    {
        public static AppConfig CreateConfiguration()
        {
            return new AppConfig
            {
                ConnectionString = CreateConnectionString(),
                UserPoolId = Environment.GetEnvironmentVariable("COGNITO_USER_POOL_ID"),
                ClientId = Environment.GetEnvironmentVariable("COGNITO_CLIENT_ID"),
                JwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET"),
                AwsAccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID_COGNITO"),
                AwsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY_COGNITO"),
                AwsSessionToken = Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN_COGNITO"),
                AwsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1",
                JwtExpirationHours = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_HOURS"), out int hours) ? hours : 1
            };
        }

        private static string CreateConnectionString()
        {
            string host = Environment.GetEnvironmentVariable("BD_HOST");
            string banco = Environment.GetEnvironmentVariable("BD_NOME");
            string usuario = Environment.GetEnvironmentVariable("BD_USUARIO");
            string senha = Environment.GetEnvironmentVariable("BD_SENHA");

            return $"Host={host};Database={banco};User ID={usuario};Password={senha};";
        }
    }
}