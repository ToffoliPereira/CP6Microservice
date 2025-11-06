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
    public class UsuarioService : IUsuarioService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(IConfiguration configuration, ILogger<UsuarioService> logger)
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

        public async Task SetAsync(Usuario movimentacaoEstoque, Livro produto, string key, string value, TimeSpan? expiry = null)
        {
            try
            {
                if (movimentacaoEstoque.Qtd < 0)
                {
                    _logger.LogError("Quantidade não pode ser negativa!");
                }
                else if (movimentacaoEstoque.Tipo == "SAIDA" && movimentacaoEstoque.Qtd <= 0)
                {
                    _logger.LogError("Não há produto no estoque.");
                }
                else if (produto.Categoria == "PERECIVEL" && movimentacaoEstoque.DataValidade == null)
                {
                    _logger.LogError("Produtos perecíveis devem ter data de validade.");
                }
                else
                {
                    await _database.StringSetAsync(key, value, expiry);
                    _logger.LogDebug("Movimentação salvs com sucesso!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao definir chave {Key} no Redis", key);
            }
        }

        public async Task DeleteAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
                _logger.LogDebug("Chave {Key} removida do Redis com sucesso", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover chave {Key} do Redis", key);
                throw;
            }
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência da chave {Key} no Redis", key);
                return false;
            }
        }

        public async Task<bool> SetExpiryAsync(string key, TimeSpan expiry)
        {
            try
            {
                return await _database.KeyExpireAsync(key, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao definir expiração da chave {Key} no Redis", key);
                return false;
            }
        }

        public void Dispose()
        {
            _redis?.Dispose();
        }
    }
}
