using Domain;

namespace Service
{
    public interface IEmprestimoService
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(EmprestimoService emprestimoService, Usuario usuario, Emprestimo emprestimo, string key, string value, TimeSpan? expiry = null);
    }
}