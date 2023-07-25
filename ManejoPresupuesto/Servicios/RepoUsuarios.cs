using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{

    public interface IRepoUsuarios
    {
        Task<int> CrearUsuario(Usuario usuario);
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
    }

    public class RepoUsuarios: IRepoUsuarios
    {
        private readonly string connectionString;
        public RepoUsuarios(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public async Task<int> CrearUsuario(Usuario usuario)
        {
            using var connetion = new SqlConnection(connectionString);
            var id = await connetion.QuerySingleAsync<int>(@"INSERT INTO Usuarios (Email, EmailNormalizado, PasswordHash)
                                                                VALUES (@Email, @EmailNormalizado, @PasswordHash)
                                                                SELECT SCOPE_IDENTITY();", usuario);

            return id;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var connetion = new SqlConnection(connectionString);
            return await connetion.QuerySingleOrDefaultAsync<Usuario>("Select * FROM Usuarios WHERE EmailNormalizado= @EmailNormalizado", new { emailNormalizado });
        }


    }
}