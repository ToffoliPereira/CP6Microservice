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
    public class RelatorioService : IRelatorioService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<RelatorioService> _logger;

        public RelatorioService(IConfiguration configuration, ILogger<RelatorioService> logger)
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

        public async Task<string?> GetAllAtrasoAsync(string key, Emprestimo emprestimo)
        {
            try
            {
                if (emprestimo.Status == "ATRASADO")
                {
                    var value = await _database.StringGetAsync(key);
                    return value.HasValue ? value.ToString() : null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex) {
                return null;
            }
        }


    }
}
