using System;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using AutenticacaoFunction.Models;

namespace AutenticacaoFunction.Services
{
    public interface IAutenticacaoService
    {
        string GerarJwt(Guid clienteId, string cpf, string nome);
    }
}