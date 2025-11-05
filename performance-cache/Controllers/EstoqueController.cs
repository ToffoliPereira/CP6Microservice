using Domain;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repository;
using Service;
using System.Net;

namespace performance_cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstoqueController : ControllerBase
    {
        private const string cacheKey = "estoques-cache";
        private readonly IEstoqueRepository estoqueRepository;
        private readonly ICacheService cacheService;
        private readonly ILogger<EstoqueController> logger;

        public EstoqueController(IEstoqueRepository estoqueRepository, ICacheService cacheService, ILogger<EstoqueController> logger)
        {
            this.estoqueRepository = estoqueRepository;
            this.cacheService = cacheService;
            this.logger = logger;
        }

        // GET: api/vehicle
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                logger.LogInformation("Iniciando busca de estoque");

                // Tentar buscar do cache Redis
                try
                {
                    await cacheService.SetExpiryAsync(cacheKey, TimeSpan.FromMinutes(20));
                    string? cachedEstoque = await cacheService.GetAsync(cacheKey);
                    
                    if (!string.IsNullOrEmpty(cachedEstoque))
                    {
                        logger.LogInformation("Estoque encontrados no cache Redis");
                        return Ok(cachedEstoque);
                    }
                }
                catch (Exception redisEx)
                {
                    logger.LogWarning(redisEx, "Erro ao acessar cache Redis, continuando sem cache");
                }

                // Buscar do banco de dados
                var estoqueList = await estoqueRepository.GetAllEstoqueAsync();

                if (estoqueList == null || !estoqueList.Any())
                {
                    logger.LogInformation("Nenhum estoque encontrado no banco de dados");
                    return Ok(new List<Usuario>());
                }

                // Tentar salvar no cache
                try
                {
                    var estoqueListJson = JsonConvert.SerializeObject(estoqueList);
                    await cacheService.SetAsync(cacheKey, estoqueListJson, TimeSpan.FromMinutes(20));
                    logger.LogInformation("Dados salvos no cache Redis");
                }
                catch (Exception cacheEx)
                {
                    logger.LogWarning(cacheEx, "Erro ao salvar no cache Redis, mas dados foram retornados");
                }

                logger.LogInformation("Retornando {Count} estoque", estoqueList.Count());
                return Ok(estoqueList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao buscar estoque");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "Erro interno do servidor ao buscar estoque", timestamp = DateTime.UtcNow });
            }
        }

        // POST: api/vehicle
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Usuario estoque)
        {
            try
            {
                if (estoque == null)
                {
                    logger.LogWarning("Tentativa de criar estoque com dados nulos");
                    return BadRequest(new { message = "Dados do estoque são obrigatórios", timestamp = DateTime.UtcNow });
                }

                // Validação básica dos campos obrigatórios
                if (string.IsNullOrWhiteSpace(estoque.Nome) ||
                    string.IsNullOrWhiteSpace(estoque.Email.ToString()) ||
                    string.IsNullOrWhiteSpace(estoque.Tipo))
                {
                    logger.LogWarning("Tentativa de criar estoque com campos obrigatórios vazios");
                    return BadRequest(new
                    {
                        message = "Tipo, Quantidade, Lote são campos obrigatórios",
                        timestamp = DateTime.UtcNow
                    });
                }

                logger.LogInformation("Criando novo estoque: {Tipo} {Qtd} - {Lote}", estoque.Nome, estoque.Email, estoque.Tipo);

                var newEstoque = await estoqueRepository.AddEstoqueAsync(estoque);

                if (newEstoque == null)
                {
                    logger.LogError("Falha ao criar estoque - repository retornou null");
                    return StatusCode((int)HttpStatusCode.InternalServerError,
                        new { message = "Erro interno ao criar estoque", timestamp = DateTime.UtcNow });
                }

                await InvalidateCache();
                logger.LogInformation("estoque criado com sucesso - ID: {Id}", newEstoque);

                return CreatedAtAction(nameof(Get), new { id = newEstoque }, newEstoque);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao criar estoque");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "Erro interno do servidor ao criar estoque", timestamp = DateTime.UtcNow });
            }
        }

        // PUT: api/vehicle/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Usuario estoque)
        {
            try
            {
                if (id <= 0)
                {
                    logger.LogWarning("Tentativa de atualizar estoque com ID inválido: {Id}", id);
                    return BadRequest(new { message = "ID do estoque deve ser maior que zero", timestamp = DateTime.UtcNow });
                }

                if (estoque == null)
                {
                    logger.LogWarning("Tentativa de atualizar estoque com dados nulos para ID: {Id}", id);
                    return BadRequest(new { message = "Dados do estoque são obrigatórios", timestamp = DateTime.UtcNow });
                }

                // Validação básica dos campos obrigatórios
                if (string.IsNullOrWhiteSpace(estoque.Nome) ||
                    string.IsNullOrWhiteSpace(estoque.Email.ToString()) ||
                    string.IsNullOrWhiteSpace(estoque.Tipo))
                {
                    logger.LogWarning("Tentativa de atualizar estoque com campos obrigatórios vazios para ID: {Id}", id);
                    return BadRequest(new
                    {
                        message = "Tipo, Quantedade, Lote são campos obrigatórios",
                        timestamp = DateTime.UtcNow
                    });
                }

                estoque.Id = id;
                logger.LogInformation("Atualizando estoque ID: {Id} - {Tipo} {Qtd} - {Lote}", id, estoque.Nome, estoque.Email, estoque.Tipo);

                await estoqueRepository.UpdateEstoqueAsync(estoque);

                await InvalidateCache();
                logger.LogInformation("Veículo atualizado com sucesso - ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao atualizar estoque ID: {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "Erro interno do servidor ao atualizar estoque", timestamp = DateTime.UtcNow });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    logger.LogWarning("Tentativa de excluir estoque com ID inválido: {Id}", id);
                    return BadRequest(new { message = "ID do estoque deve ser maior que zero", timestamp = DateTime.UtcNow });
                }

                logger.LogInformation("Excluindo estoque ID: {Id}", id);

                await estoqueRepository.DeleteEstoqueAsync(id);

                await InvalidateCache();
                logger.LogInformation("estoque excluído com sucesso - ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao excluir estoque ID: {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "Erro interno do servidor ao excluir estoque", timestamp = DateTime.UtcNow });
            }
        }

        private async Task InvalidateCache()
        {
            try
            {
                await cacheService.DeleteAsync(cacheKey);
                logger.LogInformation("Cache invalidado com sucesso");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Erro ao invalidar cache Redis, mas operação continuará");
            }
        }
    }
}
