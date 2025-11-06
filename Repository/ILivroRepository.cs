using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface ILivroRepository
    {
        Task<IEnumerable<Livro>> GetAllLivroAsync();
        Task<int> AddLivroAsync(Livro estoque);
        Task UpdateLivroAsync(Livro estoque);
        Task DeleteLivroAsync(int id);
    }
}