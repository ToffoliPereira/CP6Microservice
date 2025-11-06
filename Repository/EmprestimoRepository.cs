using Dapper;
using Domain;
using MySqlConnector;

namespace Repository
{
    public class EmprestimoRepository : IEmprestimoRepository
    {
        private readonly MySqlConnection _connection;
        public EmprestimoRepository(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
        }

        public async Task<IEnumerable<Emprestimo>> GetAllEstoqueAsync()
        {
            await _connection.OpenAsync();
            string sql = "SELECT IdEmprestimo, IsbnLivro, IdUsuario, DataEmprestimo, DataPrevDevolucao, DataRealDevolucao, Status FROM emprestimos;";
            var estoques = await _connection.QueryAsync<Emprestimo>(sql);
            await _connection.CloseAsync();
            return estoques;
        }

        public async Task<int> AddEstoqueAsync(Emprestimo estoque)
        {
            if (estoque == null)
                throw new ArgumentNullException(nameof(estoque), "Estoque inválido.");
            await _connection.OpenAsync();
            string sql = @"
                INSERT INTO emprestimos (IsbnLivro, IdUsuario, DataEmprestimo, DataPrevDevolucao, DataRealDevolucao, Status)
                VALUES (@IsbnLivro, @IdUsuario, @DataEmprestimo, @DataPrevDevolucao, @DataRealDevolucao, @Status);
                SELECT LAST_INSERT_ID();
            ";
            var id = await _connection.ExecuteScalarAsync<int>(sql, estoque);
            await _connection.CloseAsync();
            return id;
        }

        public async Task UpdateEstoqueAsync(Emprestimo estoque)
        {
            if (estoque == null || estoque.IdEmprestimo <= 0)
                throw new ArgumentException("Estoque inválido.", nameof(estoque));
            await _connection.OpenAsync();
            string sql = @"
                UPDATE emprestimos
                SET IsbnLivro = @IsbnLivro, IdUsuario = @IdUsuario, DataEmprestimo = @DataEmprestimo, DataPrevDevolucao = @DataPrevDevolucao, DataRealDevolucao = @DataPrevDevolucao, Status = @Status
                WHERE IdEmprestimo = @IdEmprestimo;
            ";
            await _connection.ExecuteAsync(sql, estoque);
            await _connection.CloseAsync();
        }

        public async Task DeleteEstoqueAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID inválido.", nameof(id));
            await _connection.OpenAsync();
            string sql = "DELETE FROM estoque WHERE idEmprestimo = @IdEmprestimo;";
            await _connection.ExecuteAsync(sql, new { Id = id });
            await _connection.CloseAsync();
        }
    }
}
