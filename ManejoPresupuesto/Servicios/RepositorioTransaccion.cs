using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography.Xml;

namespace ManejoPresupuesto.Servicios
{

    public interface IRespositorioTransaccion
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
    }

    public class RepositorioTransaccion: IRespositorioTransaccion
    {
        private readonly string connectionString;

        public RepositorioTransaccion(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public async Task Crear(Transaccion transaccion)
        {
            using var connetion = new SqlConnection(connectionString);
            var id = await connetion.QuerySingleAsync<int>("Transacciones_Insertar",
                new { 
                    transaccion.UsuarioId, 
                    transaccion.FechaTransaccion, 
                    transaccion.Monto,
                    transaccion.CategoriaId, 
                    transaccion.CuentaId, 
                    transaccion.Nota }, 
                commandType: System.Data.CommandType.StoredProcedure);
        }



        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connetion = new SqlConnection(connectionString);
            await connetion.ExecuteAsync("Transacciones_Actualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                }, commandType: System.Data.CommandType.StoredProcedure);

        }


        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connetion = new SqlConnection(connectionString);
            return await connetion.QueryFirstOrDefaultAsync<Transaccion>(@"SELECT Transacciones.*, cat.TipoOperacionId FROM Transacciones INNER JOIN Categorias cat
	                                                                    ON cat.Id = Transacciones.CategoriaId
	                                                                    WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId", 
                                                                        new { id, usuarioId });
            
        }

        public async Task Borrar(int id)
        {
            using var connetion = new SqlConnection(connectionString);
            await connetion.ExecuteAsync("Transacciones_Borrar", new { id }, commandType: System.Data.CommandType.StoredProcedure);
        }


        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionPorCuenta modelo) 
        {
            using var connetion = new SqlConnection(connectionString);
            return await connetion.QueryAsync<Transaccion>(@"SELECT t.Id, t.Monto, t.FechaTransaccin, c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId
                                                                            FROM Transacciones t
                                                                            INNER JOIN Categorias c
                                                                            ON c.Id = t.CategoriaId
                                                                            INNER JOIN Cuentas cu
                                                                            ON cu.Id = t.CuentaId
                                                                            WHERE t.CuentaId = @CuentaId and t.UsuarioId = @UsuarioId
                                                                            and FechaTransaccin BETWEEN @FechaInicio AND @FechaFin", modelo);
        }




    }

    
}
