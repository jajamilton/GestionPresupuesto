
namespace ManejoPresupuesto.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public String Email { get; set; }
        public string EmailNormalizado { get; set; }
        public string PasswordHash { get; set; }
        
    }
}