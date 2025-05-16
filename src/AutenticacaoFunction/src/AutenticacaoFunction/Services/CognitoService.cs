using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using AutenticacaoFunction.Configuration;

namespace AutenticacaoFunction.Services
{
    public class CognitoService : ICognitoService
    {
        private readonly string _userPoolId;
        private readonly string _clientId;
        private readonly string _awsAccessKeyId;
        private readonly string _awsSecretAccessKey;
        private readonly string _awsSessionToken;
        private readonly string _awsRegion;

        public CognitoService(AppConfig config)
        {
            _userPoolId = config.UserPoolId;
            _clientId = config.ClientId;
            _awsAccessKeyId = config.AwsAccessKeyId;
            _awsSecretAccessKey = config.AwsSecretAccessKey;
            _awsSessionToken = config.AwsSessionToken;
            _awsRegion = config.AwsRegion;
        }

        public async Task VerificarOuCriarUsuario(string cpf, string nome, string email, ILambdaContext context)
        {
            context.Logger.LogLine($"Verificando/criando usuário no Cognito para CPF: {cpf}");

            var cognitoClient = CreateCognitoClient();
            var username = FormatUsername(cpf);

            try
            {
                // Verificar se o usuário já existe
                var getUserRequest = new AdminGetUserRequest
                {
                    UserPoolId = _userPoolId,
                    Username = username
                };

                await cognitoClient.AdminGetUserAsync(getUserRequest);
                context.Logger.LogLine("Usuário já existe no Cognito");
            }
            catch (UserNotFoundException)
            {
                // Usuário não existe, criar novo
                await CriarNovoUsuario(cognitoClient, username, cpf, nome, email, context);
            }
        }

        private async Task CriarNovoUsuario(
            IAmazonCognitoIdentityProvider cognitoClient, 
            string username, 
            string cpf, 
            string nome, 
            string email, 
            ILambdaContext context)
        {
            context.Logger.LogLine("Criando novo usuário no Cognito");

            var attributes = new List<AttributeType>
            {
                new AttributeType { Name = "custom:cpf", Value = cpf },
                new AttributeType { Name = "name", Value = nome }
            };

            if (!string.IsNullOrEmpty(email))
            {
                attributes.Add(new AttributeType { Name = "email", Value = email });
                attributes.Add(new AttributeType { Name = "email_verified", Value = "true" });
            }

            var createUserRequest = new AdminCreateUserRequest
            {
                UserPoolId = _userPoolId,
                Username = username,
                TemporaryPassword = GenerareSenhaSegura(),
                MessageAction = MessageActionType.SUPPRESS, // Não enviar emails
                UserAttributes = attributes
            };

            await cognitoClient.AdminCreateUserAsync(createUserRequest);

            // Confirmar usuário para não precisar trocar senha
            var setPasswordRequest = new AdminSetUserPasswordRequest
            {
                UserPoolId = _userPoolId,
                Username = username,
                Password = GenerareSenhaSegura(),
                Permanent = true
            };

            await cognitoClient.AdminSetUserPasswordAsync(setPasswordRequest);
        }

        private IAmazonCognitoIdentityProvider CreateCognitoClient()
        {
            var credentials = new SessionAWSCredentials(
                _awsAccessKeyId,
                _awsSecretAccessKey,
                _awsSessionToken
            );

            return new AmazonCognitoIdentityProviderClient(
                credentials, 
                Amazon.RegionEndpoint.GetBySystemName(_awsRegion)
            );
        }

        private string FormatUsername(string cpf)
        {
            return $"CPF_{cpf}";
        }

        private string GenerareSenhaSegura()
        {
            // Gerar uma senha mais segura que Guid.NewGuid().ToString()
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }
    }
}