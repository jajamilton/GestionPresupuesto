using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly IRepositorioCtagorias repositorioCtagorias;
        private readonly IServiciosUsuarios serviciosUsuarios;

        public CategoriasController(IRepositorioCtagorias repositorioCtagorias,
            IServiciosUsuarios serviciosUsuarios)
        {
            this.repositorioCtagorias = repositorioCtagorias;
            this.serviciosUsuarios = serviciosUsuarios;
        }


        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {

            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var usuarioId = serviciosUsuarios.ObtenerUusarioId();
            categoria.UsuarioId = usuarioId;
            await repositorioCtagorias.Crear(categoria);
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Index()
        {
            var usuarioId = serviciosUsuarios.ObtenerUusarioId();
            var categorias = await repositorioCtagorias.Obtener(usuarioId);
            return View(categorias);
        }

        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = serviciosUsuarios.ObtenerUusarioId();
            var categorias = await repositorioCtagorias.ObtenerId(id, usuarioId);

            if(categorias is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categorias);
        }


        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoriaEditar)
        {
            if (!ModelState.IsValid)
            {
                return View(categoriaEditar);
            }

            var usuarioId = serviciosUsuarios.ObtenerUusarioId();
            var categorias = await repositorioCtagorias.ObtenerId(categoriaEditar.Id, usuarioId);

            if (categorias is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            categoriaEditar.UsuarioId = usuarioId;
            await repositorioCtagorias.Actualizar(categoriaEditar);
            return RedirectToAction("Index");
        }



        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = serviciosUsuarios.ObtenerUusarioId();
            var categorias = await repositorioCtagorias.ObtenerId(id, usuarioId);

            if (categorias is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categorias);
        }


        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int Id)
        { 
            var usuarioId = serviciosUsuarios.ObtenerUusarioId();
            var categorias = await repositorioCtagorias.ObtenerId(Id, usuarioId);

            if (categorias is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCtagorias.Eliminar(Id);
            return RedirectToAction("Index");
        }



    }
}
