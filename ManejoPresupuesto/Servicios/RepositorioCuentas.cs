using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(CuentaCreacionViewModel cuentaEditar);
        Task Borrar(int id);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }


    public class RepositorioCuentas: IRepositorioCuentas
    {
        private readonly string connectionString;

        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                "INSERT INTO Cuentas (Nombre, TipoCuenta, Descripcion, Balance) VALUES (@Nombre, @TipoCuentaId, @Descripcion, @Balance);SELECT SCOPE_IDENTITY();", new { nombre = cuenta.Nombre, tipocuentaid = cuenta.TipoCuentaId, descripcion = cuenta.Descripcion, balance = cuenta.Balance });


            cuenta.Id = id;

        }


        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Cuenta>(@"SELECT Cuentas.Id, Cuentas.Nombre, Balance, TC.Nombre as TipoCuenta FROM Cuentas INNER JOIN TiposCuentas tc ON TC.Id = Cuentas.TipoCuenta WHERE tc.UsuarioId = @UsuarioId order by tc.Orden", new { usuarioId });

        }


        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstAsync<Cuenta>(@"SELECT Cuentas.Id, Cuentas.Nombre, Balance, tc.Id As TipoCuentaID, Cuentas.Descripcion
                FROM Cuentas INNER JOIN TiposCuentas tc ON TC.Id = Cuentas.TipoCuenta
                WHERE tc.UsuarioId = @UsuarioId
                AND Cuentas.Id = @Id", new { id, usuarioId });
        }


        public async Task Actualizar(CuentaCreacionViewModel cuentaEditar)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE Cuentas
                                            SET Nombre = @Nombre, Balance = @Balance, Descripcion = @Descripcion,
                                            TipoCuenta = @TipoCuentaId
                                            WHERE Id = @Id;", cuentaEditar);
        }
            

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE Cuentas WHERE Id = @Id", new { id });
        }


}

    
}
