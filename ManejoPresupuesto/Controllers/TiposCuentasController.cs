using Dapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Runtime.InteropServices;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController: Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServiciosUsuarios serviciosUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServiciosUsuarios serviciosUsuarios) 
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.serviciosUsuarios = serviciosUsuarios;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = serviciosUsuarios.ObtenerUusarioId();
            var tiposCuentas = await repositorioTiposCuentas.Obtener(usuarioId);
            return View(tiposCuentas);
        }



        public IActionResult Crear()
        {
           return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = serviciosUsuarios.ObtenerUusarioId(); ;

            var yaExisteTipoCUENTA = await repositorioTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);

            if (yaExisteTipoCUENTA)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} aya existe");
                return View(tipoCuenta);
            }


            await repositorioTiposCuentas.Crear(tipoCuenta);

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = serviciosUsuarios.ObtenerUusarioId();
            var YaExisteTipoCuenta = await repositorioTiposCuentas.Existe(nombre, usuarioId);

            if (YaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }

            return Json(true);

        }

    }
}
