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
}