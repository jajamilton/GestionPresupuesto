using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using System.Transactions;
using System.Data;
using DataTable = System.Data.DataTable;
using ClosedXML.Excel;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController: Controller
    {
        private readonly IServiciosUsuarios servicios;
        private readonly IRespositorioTransaccion respositorioTransaccion;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCtagorias repositorioCtagorias;
        private readonly IMapper mapper;
        private readonly IServicioReportes servicioReportes;

        public TransaccionesController(IServiciosUsuarios servicios, 
            IRespositorioTransaccion respositorioTransaccion, 
            IRepositorioCuentas repositorioCuentas,
            IRepositorioCtagorias repositorioCtagorias,
            IMapper mapper,
            IServicioReportes servicioReportes)
        {
            this.servicios = servicios;
            this.respositorioTransaccion = respositorioTransaccion;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCtagorias = repositorioCtagorias;
            this.mapper = mapper;
            this.servicioReportes = servicioReportes;
        }


        /// <summary>
        /// Controladore que muestra las transacciones para un usuario que ingresa
        /// </summary>
        /// <param name="mes">mes actual</param>
        /// <param name="año">año acutal</param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int mes, int año)
        {
            /// se obtiene el usuario que ingresa para cargar las transacciones
            var usuarioId = servicios.ObtenerUusarioId();
            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladas(usuarioId,mes,año, ViewBag);

            /// se envia el modelo a la vista apr ser cargadas las transacciones registradas
            return View(modelo);
        }


        [HttpGet]   
        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicios.ObtenerUusarioId();
            var modelo = new TrasaccionViewModel();

            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categoria = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);

        }


        public async Task<IActionResult> Crear(TrasaccionViewModel modelo)
        {
            var usuarioId = servicios.ObtenerUusarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categoria = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }


            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await repositorioCtagorias.ObtenerId(modelo.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            modelo.UsuarioId = usuarioId;

            if (modelo.TipoOperacionId == TipoOperacion.Gastos)
            {
                modelo.Monto *= -1;
            }

            await respositorioTransaccion.Crear(modelo);
            return View("Index");

        }


        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(c => new SelectListItem(c.Nombre, c.Id.ToString()));


        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = servicios.ObtenerUusarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }


        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCtagorias.ObtenerCategorias(usuarioId, tipoOperacion);
            return categorias.Select(c => new SelectListItem(c.Nombre, c.Id.ToString()));


        }


        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno= null)
        {
            var usuarioId = servicios.ObtenerUusarioId();
            var transaccion = await respositorioTransaccion.ObtenerPorId(id, usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("No Encontrado", "Home");
            }

            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);

            modelo.MontoAnterior = modelo.Monto;

            if(modelo.TipoOperacionId == TipoOperacion.Gastos)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }

            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categoria = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.uralRetorno = urlRetorno;

            return View(modelo);

        }


        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = servicios.ObtenerUusarioId();

            if (!ModelState.IsValid)
            {
                modelo.Categoria = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                modelo.Cuentas = await ObtenerCuentas(usuarioId);

                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }


            var categorias = await repositorioCtagorias.ObtenerId(modelo.CategoriaId, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }


            var transaccion = mapper.Map<Transaccion>(modelo);
            if (transaccion.TipoOperacionId == TipoOperacion.Gastos) 
            {
                transaccion.Monto *= -1;
            }

            await respositorioTransaccion.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);

            if (string.IsNullOrEmpty(modelo.uralRetorno))
            {
                return RedirectToAction("Index");
            }
            else{
                return LocalRedirect(modelo.uralRetorno);
            }


        }



        public async Task<IActionResult> Borrar(int id,  string urlRetorno = null)
        {
            var usuarioId = servicios.ObtenerUusarioId();
            var transaccion = await respositorioTransaccion.ObtenerPorId(id, usuarioId);

            if(transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await respositorioTransaccion.Borrar(id);

            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }

            
        }


        /// <summary>
        /// Controlador que se ejecuta al dar cli en reporte semanal, este consulta un conjunto de modelo de tipo resultado por semana desde la base de datos
        /// </summary>
        /// <returns>la vista para ver los reporte por seman en un mes</returns>
        public async Task<IActionResult> Semanal(int mes, int año)
        {
            /// se consultan las transacciones dede la base de datos
            var usuarioId = servicios.ObtenerUusarioId();
            IEnumerable<ResultadoObtenerProSemena> transaccionesPorSemana = await servicioReportes.ObtenerReporteSemanala(usuarioId, mes, año, ViewBag);

            /// se agrupan las transacciones obtenidas por el tipo de operacion gastos o ingresos
            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana)
                                                    .Select(x => new ResultadoObtenerProSemena()
                                                    {
                                                        Semana = x.Key,
                                                        Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                                                        Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gastos).Select(x => x.Monto).FirstOrDefault(),
                                                    }).ToList();

            /// se establece el diad e inicio de la semana y dia fin de la semana
            if (año == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                año = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(año, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for (int i =0; i< diasSegmentados.Count();i++)
            {
                var semana = i + 1;
                var fechainicio = new DateTime(año, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(año, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if (grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerProSemena()
                    {
                        Semana = semana,
                        FechaFin = fechaFin,
                        FechaInicio = fechainicio,
                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechainicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }

            /// se agrupan las transacciones para cada una de las semanas
            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

            /// se carga un modelo par ser mostrado al usuario
            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            /// se carga la vista por semana con el modelo de transacciones por semana
            return View(modelo);
        }



        public async Task<IActionResult> Mensual(int año)
        {
            /// se consultan las transacciones dede la base de datos
            var usuarioId = servicios.ObtenerUusarioId();

            if(año == 0)
            {
                año = DateTime.Today.Year;
            }

            var transaccionesPorMes = await respositorioTransaccion.ObtenerPorMes(usuarioId, año);

            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes)
                .Select(x => new ResultadoObtenerPorMes()
                {
                    Mes = x.Key,
                    Ingresos = x.Where(x=>x.TipoOperacionId == TipoOperacion.Ingreso)
                    .Select(x=>x.Monto).FirstOrDefault(),
                    Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gastos)
                    .Select(x => x.Monto).FirstOrDefault()

                }).ToList();


            for(int mes=1; mes <= 12; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechareferencia = new DateTime(año, mes, 1);
                if(transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes()
                    {
                        Mes= mes,
                        FechaReferencia = fechareferencia,
                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechareferencia;
                }
            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();
            var modelo = new ReporteMensualViewModel();

            modelo.TransaccionesPorMes = transaccionesAgrupadas;
            modelo.Año = año;

            return View(modelo);
        }

        public IActionResult Excel()
        {
            return View();
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorMes(int mes, int año)
        {
            var fechaInicio = new DateTime(año, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            /// se consultan las transacciones dede la base de datos
            var usuarioId = servicios.ObtenerUusarioId();

            var transacciones = await respositorioTransaccion.ObtenerPorUsuarioId(
                new ParamerosObtenerTransaccioPorUsuario
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });


            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM yyyy")}.xlsx";

            return GeneraExcel(nombreArchivo, transacciones);

        }


        private FileResult GeneraExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
        {
            DataTable tabletransacciones = new DataTable("Transacciones");
            tabletransacciones.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto"),
            });

            foreach (var transaccion in transacciones)
            {
                tabletransacciones.Rows.Add(transaccion.FechaTransaccion,
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.TipoOperacionId);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(tabletransacciones);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);

                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheettml.sheet", 
                        nombreArchivo);
                }
            }

        }


        public IActionResult Calendario()
        {
            return View();
        }

    }
}
