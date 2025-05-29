using System;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using AutenticacaoFunction.Models;

namespace AutenticacaoFunction.Services
{
    public interface IClienteService
    {
        Task<Cliente> ObterClientePorCpf(string cpf, ILambdaContext context);
        Task AtualizarUltimoAcesso(string cpf, ILambdaContext context);
    }
}