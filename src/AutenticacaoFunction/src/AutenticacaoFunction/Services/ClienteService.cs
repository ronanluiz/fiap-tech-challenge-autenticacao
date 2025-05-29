using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using AutenticacaoFunction.Configuration;
using AutenticacaoFunction.Models;
using Npgsql;

namespace AutenticacaoFunction.Services
{
    public class ClienteService : IClienteService
    {
        private readonly string _connectionString;

        public ClienteService(AppConfig config)
        {
            _connectionString = config.ConnectionString;
        }

        public async Task<Cliente> ObterClientePorCpf(string cpf, ILambdaContext context)
        {
            context.Logger.LogLine($"Verificando cliente com CPF: {cpf}");

            await using var dataSource = CreateDataSource();
            
            const string sql = @"
                SELECT customer_id, cpf, name, email 
                FROM customer 
                WHERE cpf = @CPF";
                
            await using var command = dataSource.CreateCommand(sql);
            command.Parameters.AddWithValue("@CPF", cpf);

            await using var reader = await command.ExecuteReaderAsync();
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

        public async Task AtualizarUltimoAcesso(string cpf, ILambdaContext context)
        {
            context.Logger.LogLine($"Atualizando último acesso para CPF: {cpf}");

            await using var dataSource = CreateDataSource();

            const string sql = @"
                UPDATE customer 
                SET last_access = CURRENT_TIMESTAMP 
                WHERE cpf = @CPF";
                
            await using var command = dataSource.CreateCommand(sql);
            command.Parameters.AddWithValue("@CPF", cpf);

            await command.ExecuteNonQueryAsync();
        }

        private NpgsqlDataSource CreateDataSource()
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
            return dataSourceBuilder.Build();
        }
    }
}