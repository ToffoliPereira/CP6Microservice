using Domain;
using Repository;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service
{
    public class LivroService : ILivroService
    {
        private readonly ILivroRepository _livroRepository;
        private readonly ICacheService _cacheService;

        public LivroService(ILivroRepository livroRepository, ICacheService cacheService)
        {
            _livroRepository = livroRepository;
            _cacheService = cacheService;
        }

        // Consulta no cache, depois banco
        public async Task<Livro?> ConsultarLivroAsync(string livro)
        {
            if (string.IsNullOrWhiteSpace(livro))
                throw new ArgumentException("Insira o Livro.", nameof(livro));

            // Verifica cache
            var livroCache = await _cacheService.GetAsync(livro);
            if (livroCache != null)
            {
                return JsonSerializer.Deserialize<Livro>(livroCache);
            }

            // Verifica banco
            var livroDb = await _livroRepository.GetLivroByCodeAsync(livro);
            if (livroDb != null)
            {
                var serializedLivro = JsonSerializer.Serialize(livroDb);
                await _cacheService.SetAsync(livro, serializedLivro, TimeSpan.FromHours(1));
            }

            return livroDb;
        }

        public async Task<IEnumerable<Livro>> GetAllLivrosAsync()
        {
            return await _livroRepository.GetAllLivrosAsync();
        }
    }
}
