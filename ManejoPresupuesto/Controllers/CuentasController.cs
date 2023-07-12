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
        private readonly IServicioReportes servicioReportes;

        public CuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, 
            IServiciosUsuarios serviciosUsuarios,
            IRepositorioCuentas repositorioCuentas, 
            IMapper mapper,
            IRespositorioTransaccion respositorioTransaccion,
            IServicioReportes servicioReportes)
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            ServiciosUsuarios = serviciosUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.mapper = mapper;
            this.respositorioTransaccion = respositorioTransaccion;
            this.servicioReportes = servicioReportes;
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


        /// <summary>
        /// Metodo que carga las transacciones por la cuenta selecionada por el usuario
        /// </summary>
        /// <param name="id">id de la cuenta que el usuario selecciona y por la que se fitlran las trnsacciones</param>
        /// <param name="mes">mes actual para a su vez fitlrar las transacciones</param>
        /// <param name="año">mes actual para a su vez fitlrar las transacciones</param>
        /// <returns>muestra las transacciones para una cuenta seleccionada</returns>
        [HttpGet]
        public async Task<IActionResult> Detalle(int id, int mes, int año)
        {
            /// Se ontiene el id del usuario que ingresa y se consutla si la cuenta selecionada existe
            var UsuarioID = ServiciosUsuarios.ObtenerUusarioId();
            var cuenta = await repositorioCuentas.ObtenerPorId(id, UsuarioID);

            /// se carga el nombre de la cuenta selecioanda para cargar a nivel de interfaz
            ViewBag.Nombre = cuenta.Nombre;

            /// se consutlan las transacciones y se envian a la vista para ser mostradas
            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladasProCuenta(UsuarioID, id, mes, año, ViewBag);
            return View(modelo);

        }


    }
}
