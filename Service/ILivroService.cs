using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service
{
    public interface ILivroService
    {
        Task<Livro?> ConsultarLivroAsync(string livro);
        Task<IEnumerable<Livro>> GetAllLivrosAsync();
    }
}