using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using Amazon.Runtime;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AutenticacaoFunction;

public class Function
{
    private readonly string _connectionString;
    private readonly string _userPoolId;
    private readonly string _clientId;
    private readonly string _jwtSecret;
    private readonly string _awsAccessKeyId;
    private readonly string _awsSecretAccessKey;
    private readonly string _awsSessionToken;

    public Function()
    {
        // Obter as configurações do ambiente
        _connectionString = CriarConnectionString();
        _userPoolId = Environment.GetEnvironmentVariable("COGNITO_USER_POOL_ID");
        _clientId = Environment.GetEnvironmentVariable("COGNITO_CLIENT_ID");
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        _awsAccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        _awsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        _awsSessionToken = Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogLine("Recebida solicitação de autenticação por CPF");

        try
        {
            // Verificar se o método é POST
            if (request.HttpMethod != "POST")
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 405,
                    Body = JsonConvert.SerializeObject(new { message = "Método não permitido" }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            // Extrair CPF do corpo da requisição
            var requestBody = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Body);

            if (!requestBody.TryGetValue("cpf", out var cpf))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonConvert.SerializeObject(new { message = "CPF não fornecido" }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            // Validar formato do CPF (simplificado)
            cpf = cpf.Replace(".", "").Replace("-", "").Trim();
            if (cpf.Length != 11 || !long.TryParse(cpf, out _))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonConvert.SerializeObject(new { message = "CPF inválido" }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            // Verificar cliente no banco de dados
            var cliente = await ValidarCliente(cpf, context);
            if (cliente == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 404,
                    Body = JsonConvert.SerializeObject(new { message = "Cliente não encontrado" }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            // Verificar/Autenticar no Cognito (criar usuário se não existir)
            await VerificarOuCriarUsuarioCognito(cpf, cliente.Nome, cliente.Email, context);

            // Gerar token JWT
            var token = GerarJwt(cliente.Id, cpf, cliente.Nome);

            // Atualizar último acesso
            await AtualizarUltimoAcesso(cpf, context);

            // Retornar resposta de sucesso com token
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(new
                {
                    token = token,
                    cliente = new
                    {
                        id = cliente.Id,
                        nome = cliente.Nome,
                        email = cliente.Email
                    }
                }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"Erro: {ex.Message}");
            context.Logger.LogLine(ex.StackTrace);

            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonConvert.SerializeObject(new { message = "Erro interno do servidor" }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }

    private string CriarConnectionString()
    {
        string host = Environment.GetEnvironmentVariable("DB_HOST");
        string banco = Environment.GetEnvironmentVariable("DB_NAME");
        string usuario = Environment.GetEnvironmentVariable("DB_USERNAME");
        string senha = Environment.GetEnvironmentVariable("DB_PASSWORD");

        return "Host=${host};Database=${banco};User ID=${usuario};Password=${senha};";
    }

    private async Task<Cliente> ValidarCliente(string cpf, ILambdaContext context)
    {
        context.Logger.LogLine($"Verificando cliente com CPF: {cpf}");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
        var dataSource = dataSourceBuilder.Build();

        using var connection = await dataSource.OpenConnectionAsync();

        string sql = "SELECT customer_id, cpf, name, email FROM customer WHERE cpf = @CPF";
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CPF", cpf);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Cliente
            {
                Id = reader.GetGuid(0),
                CPF = reader.GetString(1),
                Nome = reader.GetString(2),
                Email = reader.IsDBNull(3) ? null : reader.GetString(3)
            };
        }

        return null;
    }

    private async Task VerificarOuCriarUsuarioCognito(string cpf, string nome, string email, ILambdaContext context)
    {
        context.Logger.LogLine($"Verificando/criando usuário no Cognito para CPF: {cpf}");

        var credentials = new SessionAWSCredentials(
            _awsAccessKeyId,
            _awsSecretAccessKey,
            _awsSessionToken
        );

        var cognitoClient = new AmazonCognitoIdentityProviderClient(credentials, Amazon.RegionEndpoint.USEast1);

        try
        {
            // Verificar se o usuário já existe
            var adminGetUserRequest = new AdminGetUserRequest
            {
                UserPoolId = _userPoolId,
                Username = $"CPF_{cpf}" // Prefixo CPF_ para evitar conflitos
            };

            await cognitoClient.AdminGetUserAsync(adminGetUserRequest);
            context.Logger.LogLine("Usuário já existe no Cognito");
        }
        catch (UserNotFoundException)
        {
            // Usuário não existe, criar novo
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

            var adminCreateUserRequest = new AdminCreateUserRequest
            {
                UserPoolId = _userPoolId,
                Username = $"CPF_{cpf}",
                TemporaryPassword = Guid.NewGuid().ToString(),
                MessageAction = MessageActionType.SUPPRESS, // Não enviar emails
                UserAttributes = attributes
            };

            await cognitoClient.AdminCreateUserAsync(adminCreateUserRequest);

            // Confirmar usuário para não precisar trocar senha
            var adminSetUserPasswordRequest = new AdminSetUserPasswordRequest
            {
                UserPoolId = _userPoolId,
                Username = $"CPF_{cpf}",
                Password = Guid.NewGuid().ToString(),
                Permanent = true
            };

            await cognitoClient.AdminSetUserPasswordAsync(adminSetUserPasswordRequest);
        }
    }

    private string GerarJwt(Guid clienteId, string cpf, string nome)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, clienteId.ToString()),
                new Claim("cpf", cpf),
                new Claim(ClaimTypes.Name, nome)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task AtualizarUltimoAcesso(string cpf, ILambdaContext context)
    {
        context.Logger.LogLine($"Atualizando último acesso para CPF: {cpf}");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
        var dataSource = dataSourceBuilder.Build();
        
        using var connection = await dataSource.OpenConnectionAsync();

        string sql = "UPDATE customers SET last_access = UTC_TIMESTAMP() WHERE cpf = @CPF";
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CPF", cpf);

        await command.ExecuteNonQueryAsync();        
    }
}

