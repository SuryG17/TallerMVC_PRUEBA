using System;
using System.ComponentModel.DataAnnotations;

namespace TallerMecanicoMVC.Models
{
    public class Cita
    {
        [Key]
        public int IdCita { get; set; }

        [Required]
        public int IdVehiculo { get; set; }  // Relación con el vehículo

        [Required]
        [Display(Name = "Fecha y Hora de la Cita")]
        public DateTime FechaHora { get; set; } // Fecha y hora de la cita

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente";  // Estados: Pendiente, Aprobada, Rechazada

        [StringLength(255)]
        public string Comentarios { get; set; }  // Observaciones de la cita

        public DateTime? FechaTerminacion { get; set; }  // Solo se usa cuando se aprueba

        // 🔹 Propiedades para mostrar información del vehículo en la vista de listado
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Placas { get; set; }
    }
}
