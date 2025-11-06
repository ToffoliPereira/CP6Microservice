using Dapper;
using Domain;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class LivroRepository : ILivroRepository
    {
        private readonly MySqlConnection _connection;

        public LivroRepository(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
        }

        public async Task<IEnumerable<Emprestimo>> GetAllLivroAsync()
        {
            await _connection.OpenAsync();
            string sql = "SELECT Isbn, Titulo, Autor, Categoria, Status, DataCadastro FROM livros;";
            var estoques = await _connection.QueryAsync<Emprestimo>(sql);
            await _connection.CloseAsync();
            return estoques;
        }

        public async Task<int> AddLivroAsync(Emprestimo estoque)
        {
            if (estoque == null)
                throw new ArgumentNullException(nameof(estoque), "Estoque inválido.");
            await _connection.OpenAsync();
            string sql = @"
                INSERT INTO livros (Isbn, Titulo, Autor, Categoria, Status, DataCadastro)
                VALUES (@Isbn, @Titulo, @Autor, @Categoria, @Status, @DataCadastro);
                SELECT LAST_INSERT_ID();
            ";
            var id = await _connection.ExecuteScalarAsync<int>(sql, estoque);
            await _connection.CloseAsync();
            return id;
        }

        public async Task UpdateLivroAsync(Emprestimo estoque)
        {
            if (estoque == null || estoque.IdEmprestimo <= 0)
                throw new ArgumentException("Estoque inválido.", nameof(estoque));
            await _connection.OpenAsync();
            string sql = @"
                UPDATE livros
                SET Titulo = @Titulo, Autor = @Autor, Categoria = @Categoria, Status = @Status, DataCadastro = @DataCadastro
                WHERE Isbn = @Isbn;
            ";
            await _connection.ExecuteAsync(sql, estoque);
            await _connection.CloseAsync();
        }

        public async Task DeleteLivroAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID inválido.", nameof(id));
            await _connection.OpenAsync();
            string sql = "DELETE FROM livros WHERE Isbn = @Isbn;";
            await _connection.ExecuteAsync(sql, new { Id = id });
            await _connection.CloseAsync();
        }
    }
}
