using Dapper;
using Domain;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly MySqlConnection _connection;

        public UsuarioRepository(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
        }

        public async Task<IEnumerable<Usuario>> GetAllProdutosAsync()
        {
            await _connection.OpenAsync();
            try
            {
                string sql = @"
                    SELECT Id, Nome, Email, Tipo, DataCadastro
                    FROM Usuarios;
                ";
                return await _connection.QueryAsync<Usuario>(sql);
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<int> AddProdutoAsync(Usuario produto)
        {
            if (produto == null)
                throw new ArgumentNullException(nameof(produto), "Produto inválido.");

            await _connection.OpenAsync();
            try
            {
                string sql = @"
                    INSERT INTO Usuarios 
                        (Nome, Email, Tipo, DataCadastro)
                    VALUES
                        (@Nome, @Email, @Tipo, @DataCadastro);
                    SELECT LAST_INSERT_ID();
                ";
                return await _connection.ExecuteScalarAsync<int>(sql, produto);
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }

        public async Task<Usuario?> GetProdutoByCodeAsync(string produto)
        {
            if (string.IsNullOrWhiteSpace(produto))
                throw new ArgumentException("Insira o produto.", nameof(produto));

            await _connection.OpenAsync();
            try
            {
                string sql = @"
                    SELECT Id, Nome, Email, Tipo, DataCadastro
                    FROM Usuarios
                    WHERE Nome = @Nome
                    LIMIT 1;
                ";
                return await _connection.QueryFirstOrDefaultAsync<Usuario>(sql);
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }
    }
}
