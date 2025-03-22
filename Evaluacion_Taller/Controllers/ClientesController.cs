using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using TallerMecanicoMVC.Models;

namespace TallerMecanicoMVC.Controllers
{
    public class ClientesController : Controller
    {
        public ActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registro(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Por favor, revise los datos ingresados.";
                return View(cliente);
            }

            // Encriptar la contraseña antes de guardarla
            cliente.Contraseña = Cliente.EncriptarMD5(cliente.Contraseña);

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["TallerMecanico"].ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SP_TM_RegistrarCliente", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                        cmd.Parameters.AddWithValue("@Apellido", cliente.Apellido);
                        cmd.Parameters.AddWithValue("@Email", cliente.Email);
                        cmd.Parameters.AddWithValue("@Telefono", cliente.Telefono);
                        cmd.Parameters.AddWithValue("@Contraseña", cliente.Contraseña);
                        cmd.Parameters.AddWithValue("@Marca", cliente.Marca);
                        cmd.Parameters.AddWithValue("@Modelo", cliente.Modelo);
                        cmd.Parameters.AddWithValue("@Año", cliente.Año);
                        cmd.Parameters.AddWithValue("@Placas", cliente.Placas);

                        cmd.ExecuteNonQuery();
                    }
                }

                ViewBag.Success = "Registro exitoso. Ahora puede iniciar sesión.";
                return RedirectToAction("Index", "Login");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 50000) // Capturar error de RAISERROR en el SP
                {
                    ViewBag.Error = ex.Message;
                }
                else
                {
                    ViewBag.Error = "Ocurrió un error inesperado.";
                }
            }
            TempData["Success"] = "✅ Registro exitoso";

            return View(cliente);
        }
    }
}
