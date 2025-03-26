using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TallerMecanicoMVC.Models
{
    public class CitaViewModel
    {
        [Required(ErrorMessage = "Seleccione un vehículo.")]
        public int IdVehiculo { get; set; }

        [Required(ErrorMessage = "Ingrese la fecha y hora de la cita.")]
        [Display(Name = "Fecha y Hora")]
        public DateTime FechaHora { get; set; }

        public IEnumerable<SelectListItem> Vehiculos { get; set; }
    }
}
