using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using TallerMecanicoMVC.Models;

namespace TallerMecanicoMVC.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return View();
            }

            string hashedPassword = Cliente.EncriptarMD5(password);

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["TallerMecanico"].ConnectionString))
            {
                con.Open();

                // Verificar si es un Cliente
                string queryCliente = "SELECT ID, Nombre FROM Clientes WHERE Email = @Email AND Contraseña = @Password";
                SqlCommand cmdCliente = new SqlCommand(queryCliente, con);
                cmdCliente.Parameters.AddWithValue("@Email", email);
                cmdCliente.Parameters.AddWithValue("@Password", hashedPassword);
                SqlDataReader readerCliente = cmdCliente.ExecuteReader();

                if (readerCliente.HasRows)
                {
                    readerCliente.Read();
                    Session["Usuario"] = readerCliente["Nombre"].ToString();
                    Session["TipoUsuario"] = "Cliente";
                    return RedirectToAction("Crear", "Citas"); // Cliente redirigido a crear cita
                }

                readerCliente.Close();

                // Verificar si es un Administrador
                string queryAdmin = "SELECT ID, Nombre FROM Administradores WHERE Email = @Email AND Contraseña = @Password";
                SqlCommand cmdAdmin = new SqlCommand(queryAdmin, con);
                cmdAdmin.Parameters.AddWithValue("@Email", email);
                cmdAdmin.Parameters.AddWithValue("@Password", hashedPassword);
                SqlDataReader readerAdmin = cmdAdmin.ExecuteReader();

                if (readerAdmin.HasRows)
                {
                    readerAdmin.Read();
                    Session["Usuario"] = readerAdmin["Nombre"].ToString();
                    Session["TipoUsuario"] = "Administrador";
                    return RedirectToAction("Index", "Citas"); // Administrador redirigido a la lista de citas
                }
            }

            ViewBag.Error = "Credenciales incorrectas";
            return View();
        }


        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
