using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionActualizacionViewModel : TrasaccionViewModel
    {
        public int CuentaAnteriorId { get; set; }
        public decimal MontoAnterior { get; set; }

        public string uralRetorno { get; set; }

    }
}
