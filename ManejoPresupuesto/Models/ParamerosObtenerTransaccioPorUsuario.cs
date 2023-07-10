using Microsoft.AspNetCore.Components.Web;

namespace ManejoPresupuesto.Models
{
    public class ParamerosObtenerTransaccioPorUsuario
    {
        public int UsuarioId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
