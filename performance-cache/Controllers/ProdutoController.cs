using Domain;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Service;

namespace performance_cache.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly IProdutoService _produtoService;
        private readonly IProdutoRepository _produtoRepository;

        public ProdutoController(IProdutoService produtoService, IProdutoRepository produtoRepository)
        {
            _produtoService = produtoService;
            _produtoRepository = produtoRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddProduto([FromBody] string produto)
        {
            if (string.IsNullOrWhiteSpace(produto))
                return BadRequest("O Produto não pode ser vazio.");

            try
            {
                // Chama o serviço que consulta o ViaCEP e salva o CEP completo no banco
                var id = await _produtoService.AdicionarProdutoAsync(produto);
                return CreatedAtAction(nameof(GetAllProdutos), new { id }, new { id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao adicionar o Produto: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProdutos()
        {
            try
            {
                var produtos = await _produtoService.GetAllProdutosAsync();
                return Ok(produtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar os Produtos: {ex.Message}");
            }
        }
    }
}