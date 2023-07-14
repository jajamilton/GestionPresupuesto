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
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año);
        Task<IEnumerable<ResultadoObtenerProSemena>> ObtenerPorSemana(ParamerosObtenerTransaccioPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParamerosObtenerTransaccioPorUsuario modelo);
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


        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParamerosObtenerTransaccioPorUsuario modelo)
        {
            using var connetion = new SqlConnection(connectionString);
            return await connetion.QueryAsync<Transaccion>(@"SELECT t.Id, t.Monto, t.FechaTransaccin, c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId
                                                                            FROM Transacciones t
                                                                            INNER JOIN Categorias c
                                                                            ON c.Id = t.CategoriaId
                                                                            INNER JOIN Cuentas cu
                                                                            ON cu.Id = t.CuentaId
                                                                            WHERE t.UsuarioId = @UsuarioId
                                                                            and FechaTransaccin BETWEEN @FechaInicio AND @FechaFin
                                                                            ORDER BY t.FechaTransaccin DESC", modelo);
        }


        /// <summary>
        /// Metodo que consulta las transacciones hechas por semana, enviando  un modelo con los parametros requeridos por la consutla
        /// estos parametros son una fecha inicio y una fecha final, que corresponderian a la fecha inicio y final de un mes para enviar las transacciones 
        /// de las semana de ese mes
        /// </summary>
        /// <param name="modelo">modelo con los parametros usuario, fecha fien, fecha inicio</param>
        /// <returns>retorna un modelo con los aprametros ya establecido a mostrar, como la semana, el monto toal por semana y el tipo de operacion</returns>
        public async Task<IEnumerable<ResultadoObtenerProSemena>> ObtenerPorSemana(ParamerosObtenerTransaccioPorUsuario modelo)
        {
            using var connetion = new SqlConnection(connectionString);
            return await connetion.QueryAsync<ResultadoObtenerProSemena>(@"SELECT DATEDIFF(d,@fechaInicio,FechaTransaccin) / 7+1 as Semana,
                                                                            sum(Monto) as Monto, cat.TipoOperacionId
                                                                            FROM Transacciones
                                                                            INNER JOIN Categorias cat
                                                                            ON cat.Id = Transacciones.CategoriaId
                                                                            WHERE Transacciones.FechaTransaccin BETWEEN @fechaInicio AND @fechaFin
                                                                            GROUP BY DATEDIFF(d,@fechaInicio,FechaTransaccin) / 7, cat.TipoOperacionId", modelo);
        }


        /// <summary>
        /// Metodoq ue consulta desde la base de datos las transacciones hecas en cada mes de año asi como el valor
        /// total de cada uno de los montos por mes
        /// </summary>
        /// <param name="usuarioId">usuario que registro las transacciones</param>
        /// <param name="año">año en el que se realizan las transacciones</param>
        /// <returns></returns>
        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año)
        {
            using var connetion = new SqlConnection(connectionString);
            return await connetion.QueryAsync<ResultadoObtenerPorMes>(@"SELECT MONTH(FechaTransaccin) as Mes,
                                                                                SUM(Monto) as Monto, cat.TipoOperacionId
                                                                                FROM Transacciones
                                                                                INNER JOIN Categorias cat
                                                                                ON cat.Id = Transacciones.CategoriaId
                                                                                WHERE Transacciones.UsuarioId = @usuarioId AND YEAR(FechaTransaccin) = @Año
                                                                                GROUP BY Month(FechaTransaccin), cat.TipoOperacionId", new {usuarioId, año});
        }



    }

    
}
