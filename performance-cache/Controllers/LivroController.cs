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
    public class LivroController : ControllerBase
    {
        private const string cacheKey = "produtos-cache";
        private readonly Livro produto;
        private readonly Emprestimo movimentacaoEstoque;
        private readonly ILivroRepository livroRepository;
        private readonly ILivroService livroService;
        private readonly ILogger<LivroController> logger;

        public LivroController(ILivroRepository livroRepository, ILivroService cacheService, ILogger<LivroController> logger)
        {
            this.livroRepository = this.livroRepository;
            this.livroService = livroService;
            this.logger = logger;
        }

        // GET: api/vehicle
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                logger.LogInformation("Iniciando busca de veículos");

                // Tentar buscar do cache Redis
                try
                {
                    await livroService.SetExpiryAsync(cacheKey, TimeSpan.FromMinutes(20));
                    string? cachedProdutos = await livroService.GetAsync(cacheKey);

                    if (!string.IsNullOrEmpty(cachedProdutos))
                    {
                        logger.LogInformation("Produtos encontrados no cache Redis");
                        return Ok(cachedProdutos);
                    }
                }
                catch (Exception redisEx)
                {
                    logger.LogWarning(redisEx, "Erro ao acessar cache Redis, continuando sem cache");
                }

                // Buscar do banco de dados
                var produtoList = await livroRepository.GetAllProdutosAsync();

                if (produtoList == null || !produtoList.Any())
                {
                    logger.LogInformation("Nenhum veículo encontrado no banco de dados");
                    return Ok(new List<Livro>());
                }

                // Tentar salvar no cache
                try
                {
                    var produtoListJson = JsonConvert.SerializeObject(produtoList);
                    await livroService.SetAsync(movimentacaoEstoque, produto, cacheKey, produtoListJson, TimeSpan.FromMinutes(20));
                    logger.LogInformation("Dados salvos no cache Redis");
                }
                catch (Exception cacheEx)
                {
                    logger.LogWarning(cacheEx, "Erro ao salvar no cache Redis, mas dados foram retornados");
                }

                logger.LogInformation("Retornando {Count} produtos", produtoList.Count());
                return Ok(produtoList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao buscar produtos");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "Erro interno do servidor ao buscar produtos", timestamp = DateTime.UtcNow });
            }
        }

        // POST: api/produto
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Livro produto)
        {
            try
            {
                if (produto == null)
                {
                    logger.LogWarning("Tentativa de criar produto com dados nulos");
                    return BadRequest(new { message = "Dados do produto são obrigatórios", timestamp = DateTime.UtcNow });
                }

                logger.LogInformation("Criando novo produto!");

                var newProduto = await livroRepository.AddProdutoAsync(produto);

                if (newProduto == null)
                {
                    logger.LogError("Falha ao criar produto - repository retornou null");
                    return StatusCode((int)HttpStatusCode.InternalServerError,
                        new { message = "Erro interno ao criar produto", timestamp = DateTime.UtcNow });
                }

                await InvalidateCache();
                logger.LogInformation("Produto criado com sucesso - Codigo: {Codigo}", newProduto);

                return CreatedAtAction(nameof(Get), new { id = newProduto }, newProduto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao criar veículo");
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "Erro interno do servidor ao criar veículo", timestamp = DateTime.UtcNow });
            }
        }

        // PUT: api/vehicle/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Livro vehicle)
        {
            try
            {
                if (id <= 0)
                {
                    logger.LogWarning("Tentativa de atualizar veículo com ID inválido: {Id}", id);
                    return BadRequest(new { message = "ID do veículo deve ser maior que zero", timestamp = DateTime.UtcNow });
                }

                if (vehicle == null)
                {
                    logger.LogWarning("Tentativa de atualizar veículo com dados nulos para ID: {Id}", id);
                    return BadRequest(new { message = "Dados do veículo são obrigatórios", timestamp = DateTime.UtcNow });
                }

                vehicle.Codigo = id;
                logger.LogInformation("Atualizando produto");

                await livroRepository.UpdateProdutoAsync(vehicle);

                await InvalidateCache();
                logger.LogInformation("Veículo atualizado com sucesso - ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao atualizar veículo ID: {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "Erro interno do servidor ao atualizar veículo", timestamp = DateTime.UtcNow });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    logger.LogWarning("Tentativa de excluir veículo com ID inválido: {Id}", id);
                    return BadRequest(new { message = "ID do veículo deve ser maior que zero", timestamp = DateTime.UtcNow });
                }

                logger.LogInformation("Excluindo veículo ID: {Id}", id);

                await livroRepository.DeleteProdutoAsync(id);

                await InvalidateCache();
                logger.LogInformation("Veículo excluído com sucesso - ID: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao excluir veículo ID: {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new { message = "Erro interno do servidor ao excluir veículo", timestamp = DateTime.UtcNow });
            }
        }

        private async Task InvalidateCache()
        {
            try
            {
                await livroService.DeleteAsync(cacheKey);
                logger.LogInformation("Cache invalidado com sucesso");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Erro ao invalidar cache Redis, mas operação continuará");
            }
        }
    }