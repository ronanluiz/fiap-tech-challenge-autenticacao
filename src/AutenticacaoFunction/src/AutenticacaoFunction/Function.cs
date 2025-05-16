using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using AutenticacaoFunction.Models;
using AutenticacaoFunction.Services;
using AutenticacaoFunction.Configuration;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AutenticacaoFunction;

public class Function
{
    private readonly IClienteService _clienteService;
    private readonly ICognitoService _cognitoService;
    private readonly IAutenticacaoService _autenticacaoService;
    private readonly IHttpClientWrapper _httpClient;

    public Function() : this(null, null, null, null) { }

    public Function(
        IClienteService clienteService = null,
        ICognitoService cognitoService = null,
        IAutenticacaoService autenticacaoService = null,
        IHttpClientWrapper httpClient = null)
    {
        var appConfig = ConfigurationFactory.CreateConfiguration();

        _clienteService = clienteService ?? new ClienteService(appConfig);
        _cognitoService = cognitoService ?? new CognitoService(appConfig);
        _autenticacaoService = autenticacaoService ?? new AutenticacaoService(appConfig);
        _httpClient = httpClient ?? new HttpClientWrapper();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogLine("Recebida solicitação de autenticação por CPF");

        try
        {
            if (request.HttpMethod != "POST")
            {
                return CreateResponse(405, new { message = "Método não permitido" });
            }

            // Extrair e validar CPF
            var requestBody = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Body ?? "{}");
            
            if (!requestBody.TryGetValue("cpf", out var cpf))
            {
                return CreateResponse(400, new { message = "CPF não fornecido" });
            }

            var cpfLimpo = CpfValidator.CleanCpf(cpf);
            if (!CpfValidator.IsValid(cpfLimpo))
            {
                return CreateResponse(400, new { message = "CPF inválido" });
            }

            // Verificar cliente no banco de dados
            var cliente = await _clienteService.ObterClientePorCpf(cpfLimpo, context);
            if (cliente == null)
            {
                return CreateResponse(404, new { message = "Cliente não encontrado" });
            }

            // Verificar/Autenticar no Cognito
            await _cognitoService.VerificarOuCriarUsuario(cpfLimpo, cliente.Nome, cliente.Email, context);

            // Gerar token JWT
            var token = _autenticacaoService.GerarJwt(cliente.Id, cpfLimpo, cliente.Nome);

            // Atualizar último acesso
            await _clienteService.AtualizarUltimoAcesso(cpfLimpo, context);

            // Retornar resposta de sucesso com token
            return CreateResponse(200, new
            {
                token,
                cliente = new
                {
                    id = cliente.Id,
                    nome = cliente.Nome,
                    email = cliente.Email
                }
            });
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"Erro: {ex.Message}");
            context.Logger.LogLine(ex.StackTrace);

            return CreateResponse(500, new { message = "Erro interno do servidor" });
        }
    }

    private APIGatewayProxyResponse CreateResponse(int statusCode, object body)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Body = JsonConvert.SerializeObject(body),
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }
}