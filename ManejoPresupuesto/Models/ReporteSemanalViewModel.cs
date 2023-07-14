namespace ManejoPresupuesto.Models
{
    /// <summary>
    /// modelo que sera mostrado del lado de la vista
    /// </summary>
    public class ReporteSemanalViewModel
    {
        /// <summary>
        ///  se suman los ingresos para las transacciones por semana igual dque los gastos y el total
        /// </summary>
        public decimal Ingresos => TransaccionesPorSemana.Sum(x => x.Ingresos);
        public decimal Gastos => TransaccionesPorSemana.Sum(x => x.Gastos);
        public decimal Total => Ingresos - Gastos;
        public DateTime FechaReferencia { get; set; }

        /// <summary>
        ///  transacciones por semana ya aggrupadas 
        /// </summary>
        public IEnumerable<ResultadoObtenerProSemena> TransaccionesPorSemana { get; set; }

    }
}
