using Domain;

namespace Service
{
    public interface IUsuarioService
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(MovimentacaoEstoque movimentacaoEstoque, Produto produto, string key, string value, TimeSpan? expiry = null);
        Task DeleteAsync(string key);
        Task<bool> KeyExistsAsync(string key);
        Task<bool> SetExpiryAsync(string key, TimeSpan expiry);
    }
}