using System.ComponentModel.DataAnnotations;

namespace TallerMecanicoMVC.Models
{
    public class Vehiculo
    {
        [Key]
        public int IdVehiculo { get; set; }

        [Required]
        public int ClienteID { get; set; }  // Relación con Cliente

        [Required]
        public string Marca { get; set; }

        [Required]
        public string Modelo { get; set; }

        [Required]
        public int Año { get; set; }

        [Required]
        public string Placas { get; set; }
    }
}
