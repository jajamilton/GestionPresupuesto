using ManejoPresupuesto.Models;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using Microsoft.AspNetCore.Http;


namespace ManejoPresupuesto.Servicios
{
    /// <summary>
    /// Interfaz para la clase creada la cual nos permite luego realizar inyeccion de dependencias y acceder a los metodos por medio de la interfaz
    /// </summary>
    public interface IServicioReportes
    {
        Task<IEnumerable<ResultadoObtenerProSemena>> ObtenerReporteSemanala(int usuaioId, int mes, int año, dynamic viewbag);
        Task<ReporteTransaccionesDetalledas> ObtenerReporteTransaccionesDetalladas(int usuarioId, int mes, int año, dynamic Viewbag);
        Task<ReporteTransaccionesDetalledas> ObtenerReporteTransaccionesDetalladasProCuenta(int usuarioId, int cuentaId, int mes, int año, dynamic Viewbag);
    }


    /// <summary>
    /// Clase que implementa la interfaz, en esta clase se almacenan los metodo para el cargue de losreportes
    /// ya sean los generales como los reportes por la cuenta selecioanda
    /// </summary>
    public class ServicioReportes: IServicioReportes
    {

        /// <summary>
        /// Inyeccion de dependencias para la clase de repositorio ransacciones el cual permite consultar datos de las transacciones
        /// tambien la inyeccion de la libreria httpcontez, esto paa poder ser usado estos sericio a lo largo de la clase
        /// </summary>
        private readonly IRespositorioTransaccion repositorioTransaccion;
        private readonly HttpContext httpContext;


        public ServicioReportes(IRespositorioTransaccion repositorioTransaccion, IHttpContextAccessor httpContextAccessor)
        {
            
            this.repositorioTransaccion = repositorioTransaccion;
            this.httpContext = httpContextAccessor.HttpContext;
        }


        /// <summary>
        /// Metodoq eu consulta reporte por semana de un mes
        /// </summary>
        /// <param name="usuaioId">usuari que registra las transacciones</param>
        /// <param name="mes">mes del año para general el reporte</param>
        /// <param name="año"></param>
        /// <param name="viewbag"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ResultadoObtenerProSemena>> ObtenerReporteSemanala(int usuaioId, int mes, int año, dynamic viewbag)
        {
            /// se obtienen la fecha actual el mes y el año
            /// en este caso el dia incial de un mes y la fecha fin del mes
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioFin(mes, año);

            ///se crear un obejtio comn los parametros para consulta las ttransacciones
            var parametro = new ParamerosObtenerTransaccioPorUsuario()
            {
                UsuarioId = usuaioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            AsignarValorsAlViewBag(viewbag, fechaInicio);
            var modelo = await repositorioTransaccion.ObtenerPorSemana(parametro);
            return modelo;
        }



        /// <summary>
        /// Metodo que permite consulta las transacciones en general, solo filtrando por el usuario y una fecja especifica
        /// </summary>
        /// <param name="usuarioId">usuario que ocnsulta las transacciones</param>
        /// <param name="mes">mes en el que se realizaron las transacciones</param>
        /// <param name="año">año en el que se ealizan las transacciones</param>
        /// <param name="Viewbag">datos traidos desde la interzas y que luego son enviado procesados</param>
        /// <returns>returna un modelo de tipo reportestransaccion detallada para ser mostrado a nivel de intefaz</returns>
        public async Task<ReporteTransaccionesDetalledas> ObtenerReporteTransaccionesDetalladas(int usuarioId, int mes, int año, dynamic Viewbag)
        {
            /// se obtienen la fecha actual el mes y el año
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioFin(mes, año);

            ///se crear un obejtio comn los parametros para consulta las ttransacciones
            var parametro = new ParamerosObtenerTransaccioPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            /// se consutlan las transacciones con los parametrso cargados
            var transacciones = await repositorioTransaccion.ObtenerPorUsuarioId(parametro);

            /// se genera el modelo listo para ser enviado a nivel de la interfaz
            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);

            /// se asignand los viewbag para ser cargado en la intefaz
            AsignarValorsAlViewBag(Viewbag, fechaInicio);

            /// se retorna el modelo listo para cargar
            return modelo;

        }


        /// <summary>
        /// Metodo que carga las transacciones por una cuenta selecioanda por el usuariod esde la interfaz
        /// </summary>
        /// <param name="usuarioId">usuaio que registro las transacciones</param>
        /// <param name="cuentaId">cuenta por la que se filtran las trnsacciones</param>
        /// <param name="mes">mes por el que se filtran las transacciones</param>
        /// <param name="año">año por el que se filtran las transacciones</param>
        /// <param name="Viewbag">grupo de viebag par se renbviado a la interfaz</param>
        /// <returns>modelo listo para ser mostrado en la interfaz</returns>
        public async Task<ReporteTransaccionesDetalledas> ObtenerReporteTransaccionesDetalladasProCuenta(int usuarioId, int cuentaId, int mes, int año, dynamic Viewbag)
        {
            /// se cargan el mes y año acutal o el que el usuario seleccionado
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechaInicioFin(mes, año);

            /// se establecen los parametros para ser enviado a la consulta
            var obtenerTransaccionesPorCuenta = new ObtenerTransaccionPorCuenta()
            {
                CuentaId = cuentaId,
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            /// se consultan las transacciones segun los parametros
            var transacciones = await repositorioTransaccion.ObtenerPorCuentaId(obtenerTransaccionesPorCuenta);

            /// se genera el modelo que sera mostrado a nivel de la interfaz
            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);

            /// se cargan los valores del viewbag para ser enviado a la interfaz
            AsignarValorsAlViewBag(Viewbag, fechaInicio);

            /// se envia el modelo
            return modelo;

        }


        /// <summary>
        /// metodo para asignar los valores del viewbag en este caso el sitio al que pertenece
        /// y las fechas por las que se filtran y muestran a nivel de la interfaz
        /// </summary>
        /// <param name="Viewbag"></param>
        /// <param name="fechaInicio"></param>
        private void AsignarValorsAlViewBag(dynamic Viewbag, DateTime fechaInicio)
        {
            Viewbag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            Viewbag.añoAnterior = fechaInicio.AddMonths(-1).Year;

            Viewbag.mesPosterior = fechaInicio.AddMonths(1).Month;
            Viewbag.añoPosterior = fechaInicio.AddMonths(1).Year;
            Viewbag.urlRetorno = httpContext.Request.Path + httpContext.Request.QueryString;
        }



        /// <summary>
        /// Metodo para generar el modelo que sera mostrado en la interfaz, con la informaicon consutlada
        /// </summary>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <param name="transacciones"></param>
        /// <returns></returns>
        private static ReporteTransaccionesDetalledas GenerarReporteTransaccionesDetalladas(DateTime fechaInicio, DateTime fechaFin, IEnumerable<Transaccion> transacciones)
        {
            var modelo = new ReporteTransaccionesDetalledas();

            /// se filtran las transacciones por la fecha de la transaccione
            var transaccionPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                .GroupBy(x => x.FechaTransaccion)
                .Select(grupo => new ReporteTransaccionesDetalledas.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                });

            modelo.TransaccionAgrupadas = transaccionPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;

            /// se devuvle el modelo listo para mostrar
            return modelo;
        }




        /// <summary>
        /// Metodo para carga los valores de la fecha inicio y fin
        /// </summary>
        /// <param name="mes">mes actual</param>
        /// <param name="año">año actual</param>
        /// <returns></returns>
        private (DateTime fechaInicio, DateTime fechaFin) GenerarFechaInicioFin(int mes, int año)
        {
            DateTime fechaInicio;
            DateTime fechaFin;

            if (mes <= 0 || mes > 12 || año <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(año, mes, 1);
            }

            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            return (fechaInicio, fechaFin);
        }

    }
}
