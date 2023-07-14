namespace ManejoPresupuesto.Models
{

    /// <summary>
    /// Clase modelo utilizado para obtener la informacion  que se obtiene
    /// desde la base de datos y que luego se procesa para enviar a la vista
    /// </summary>
    public class ResultadoObtenerProSemena
    {
        public int Semana { get; set; }
        public decimal Monto { get; set; }
        public TipoOperacion TipoOperacionId { get; set;}
        public decimal Ingresos { get; set; }
        public decimal Gastos { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

    }
}
