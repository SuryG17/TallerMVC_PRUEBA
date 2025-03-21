using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace TallerMecanicoMVC.Models
{
    public class Cliente
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [RegularExpression(@"^[a-zA-ZÁÉÍÓÚáéíóúñÑ\s]+$", ErrorMessage = "El nombre solo debe contener letras")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [RegularExpression(@"^[a-zA-ZÁÉÍÓÚáéíóúñÑ\s]+$", ErrorMessage = "El apellido solo debe contener letras")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un email válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Debe ingresar un número de 10 dígitos")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "Debe tener al menos 6 caracteres")]
        public string Contraseña { get; set; }

        // Datos del vehículo

        [Required(ErrorMessage = "La marca del vehículo es obligatoria")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-]+$", ErrorMessage = "La marca solo debe contener letras, números y guiones")]
        public string Marca { get; set; }

        [Required(ErrorMessage = "El modelo del vehículo es obligatorio")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-]+$", ErrorMessage = "El modelo solo debe contener letras, números y guiones")]
        public string Modelo { get; set; }

        [Required(ErrorMessage = "El año del vehículo es obligatorio")]
        [Range(1900, 2100, ErrorMessage = "Ingrese un año válido entre 1900 y 2100")]
        public int Año { get; set; }

        [Required(ErrorMessage = "Las placas del vehículo son obligatorias")]
        [RegularExpression(@"^[A-Z0-9\-]{5,10}$", ErrorMessage = "Las placas deben tener entre 5 y 10 caracteres alfanuméricos en mayúsculas")]
        public string Placas { get; set; }

        // Método para encriptar la contraseña
        public static string EncriptarMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
