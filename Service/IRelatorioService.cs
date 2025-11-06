using Domain;

namespace Service
{
    public interface IRelatorioService
    {
        Task<string?> GetAsync(string key);
        Task<string?> GetAllAtrasoAsync(string key, Emprestimo emprestimo);
    }
}