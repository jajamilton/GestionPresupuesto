using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{

    public interface IRepositorioCtagorias
    {
        Task Actualizar(Categoria categoria);
        Task Crear(Categoria categoria);
        Task Eliminar(int Id);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId);
        Task<IEnumerable<Categoria>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacionId);
        Task<Categoria> ObtenerId(int ID, int usuarioId);
    }


    public class RepositorioCategorias: IRepositorioCtagorias
    {
        private readonly string connectionstring;

        public RepositorioCategorias(IConfiguration configuration)
        {
            connectionstring = configuration.GetConnectionString("DefaultConnection");
        }


        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionstring);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Categorias (Nombre, TipoOperacionId, UsuarioId)
                                                               values (@Nombre, @TipoOperacionId, @UsuarioId);
                                                                        SELECT SCOPE_IDENTITY();", categoria);

            categoria.Id = id;

        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId)
        {
            using var connetion = new SqlConnection(connectionstring);
            return await connetion.QueryAsync<Categoria>("" +
                "SELECT * FROM Categorias WHERE UsuarioId = @UsuarioId", new { usuarioId });
        }

        public async Task<Categoria> ObtenerId(int ID, int usuarioId)
        {
            using var connetion = new SqlConnection(connectionstring);
            return await connetion.QueryFirstAsync<Categoria>(@"
                                            SELECT *
                                            FROM Categorias
                                            WHERE Id= @Id  AND UsuarioId = @UsuarioId", new { ID, usuarioId});

        }

        public async Task<IEnumerable<Categoria>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connetion = new SqlConnection(connectionstring);
            return await connetion.QueryAsync<Categoria>(@"SELECT * 
                                                            FROM Categorias 
                                                            WHERE UsuarioId = @UsuarioId and TipoOperacionId = @tipoOperacionId", new { usuarioId, tipoOperacionId });
        }


        public async Task Actualizar(Categoria categoria)
        {
            using var connetion = new SqlConnection(connectionstring);
            await connetion.ExecuteAsync(@"UPDATE Categorias
                                            SET Nombre = @nOMBRE, TipoOperacionId = @TipoOperacionId
                                            WHERE Id = @Id", categoria);
        }


        public async Task Eliminar(int Id)
        {
            using var connetion = new SqlConnection(connectionstring);
            await connetion.ExecuteAsync("DELETE Categorias WHERE Id= @iD", new { Id });
        }


    }

    
}
