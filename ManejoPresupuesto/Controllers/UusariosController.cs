using Microsoft.AspNetCore.Mvc;
using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Controllers
{
    public class UsuarioController: Controller
    {
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if(!ModelState.IsValid)
            {
                return View(modelo);
            }
            return RedirectToAction("Index", "Transacciones");
        }

    }
    
}