using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.WebSockets;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IMapper mapper;
        private readonly IRespositorioTransaccion respositorioTransaccion;

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, 
            IServiciosUsuarios serviciosUsuarios,
            IRepositorioCuentas repositorioCuentas, 
            IMapper mapper,
            IRespositorioTransaccion respositorioTransaccion )
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            ServiciosUsuarios = serviciosUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.mapper = mapper;
            this.respositorioTransaccion = respositorioTransaccion;
        }

        public IServiciosUsuarios ServiciosUsuarios { get; }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {

            var usuarioId = ServiciosUsuarios.ObtenerUusarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            var modelo = new CuentaCreacionViewModel();

            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);


            return View(modelo);
        }


        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = ServiciosUsuarios.ObtenerUusarioId();
            var tipoCuenta = repositorioTiposCuentas.ObtenerPorId(cuenta.TipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrrado", "Home");
            }

            if (!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }

            await repositorioCuentas.Crear(cuenta);
            return RedirectToAction("Index");


        }


        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposcuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return tiposcuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }


        public async Task<IActionResult> Index()
        {
            var usuarioId = ServiciosUsuarios.ObtenerUusarioId();
            var cuentasConTipoCuenta = await repositorioCuentas.Buscar(usuarioId);

            var modelo = cuentasConTipoCuenta
                .GroupBy(x => x.TipoCuenta)
                .Select(grupo => new IndiceCuentasViewModel
                {
                    TipoCuenta = grupo.Key,
                    Cuentas = grupo.AsEnumerable()
                }).ToList();

            return View(modelo);

        }


        public async Task<IActionResult> Editar(int id)
        {
            var UsuarioID = ServiciosUsuarios.ObtenerUusarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, UsuarioID);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = mapper.Map<CuentaCreacionViewModel>(cuenta);

            modelo.TiposCuentas = await ObtenerTiposCuentas(UsuarioID);
            return View(modelo);

        }


        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuentaEditar)
        {
            var UsuarioID = ServiciosUsuarios.ObtenerUusarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(cuentaEditar.Id, UsuarioID);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(cuentaEditar.TipoCuentaId, UsuarioID);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuentas.Actualizar(cuentaEditar);
            return RedirectToAction("Index");

        }


        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var UsuarioID = ServiciosUsuarios.ObtenerUusarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, UsuarioID);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            var UsuarioID = ServiciosUsuarios.ObtenerUusarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, UsuarioID);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCuentas.Borrar(cuenta.Id);
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Detalle(int id, int mes, int año)
        {
            var UsuarioID = ServiciosUsuarios.ObtenerUusarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, UsuarioID);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            DateTime fechaInicio;
            DateTime fechaFin;

            if (mes <= 0 || mes>12 || año <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(año, mes, 1);
            }

            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            var obtenerTransaccionesPorCuenta = new ObtenerTransaccionPorCuenta()
            {
                CuentaId = id,
                UsuarioId = UsuarioID,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await respositorioTransaccion.ObtenerPorCuentaId(obtenerTransaccionesPorCuenta);

            var modelo = new ReporteTransaccionesDetalledas();
            ViewBag.Nombre = cuenta.Nombre;

            var transaccionPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                .GroupBy(x => x.FechaTransaccion)
                .Select(grupo => new ReporteTransaccionesDetalledas.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                });

            modelo.TransaccionAgrupadas = transaccionPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin=fechaFin;

            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.añoAnterior = fechaInicio.AddMonths(-1).Year;

            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
            ViewBag.añoPosterior = fechaInicio.AddMonths(1).Year;

            return View(modelo);

        }


    }
}
