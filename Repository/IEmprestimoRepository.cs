using Domain;

namespace Repository
{
    public interface IEmprestimoRepository
    {
        Task<IEnumerable<Emprestimo>> GetAllEstoqueAsync();
        Task<int> AddEstoqueAsync(Emprestimo estoque);
        Task UpdateEstoqueAsync(Emprestimo estoque);
        Task DeleteEstoqueAsync(int id);
    }
}
