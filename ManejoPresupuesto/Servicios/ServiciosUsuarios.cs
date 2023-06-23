namespace ManejoPresupuesto.Servicios
{

    public interface IServiciosUsuarios
    {
        int ObtenerUusarioId();
    }

    public class ServiciosUsuarios: IServiciosUsuarios
    {
        public int ObtenerUusarioId()
        {
            return 3;
        }
    }
}
