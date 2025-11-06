using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> GetAllProdutosAsync();
        Task<int> AddProdutoAsync(Usuario produto);
        Task<Usuario?> GetProdutoByCodeAsync(string produto);
    }
}
