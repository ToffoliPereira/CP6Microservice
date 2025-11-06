using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class EmprestimoService : IEmprestimoService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<EmprestimoService> _logger;

        public EmprestimoService(IConfiguration configuration, ILogger<EmprestimoService> logger)
        {
            _logger = logger;

            var redisConnectionString = configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";

            try
            {
                _redis = ConnectionMultiplexer.Connect(redisConnectionString);
                _database = _redis.GetDatabase();
                _logger.LogInformation("Conectado ao Redis com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao conectar ao Redis: {ConnectionString}", redisConnectionString);
            }
        }

        public async Task<string?> GetAsync(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                return value.HasValue ? value.ToString() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar chave {Key} no Redis", key);
                return null;
            }
        }

        public async Task SetAsync(EmprestimoService emprestimoService, Usuario usuario, Emprestimo emprestimo, string key, string value, TimeSpan? expiry = null)
        {
            try
            {
                if(emprestimo.Status == "ATIVO" || emprestimo.Status == "ATRASADO")
                {
                    _logger.LogError("Livro indisponível para empréstimo!");
                }
                else if(usuario.Tipo == "PROFESSOR"){
                    await _database.StringSetAsync(key, value, expiry);
                    emprestimo.DataPrevDevolucao.AddDays(10);
                    _logger.LogDebug("Empréstimo salvo com sucesso!");
                }
                else
                {
                    await _database.StringSetAsync(key, value, expiry);
                    emprestimo.DataPrevDevolucao.AddDays(5);
                    _logger.LogDebug("Empréstimo salvo com sucesso!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao definir chave {Key} no Redis", key);
            }
        }
    }
}
