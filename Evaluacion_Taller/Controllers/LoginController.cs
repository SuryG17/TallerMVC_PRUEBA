using System;
using System.Configuration;
using System.Data.SqlClient;
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

                //Verificar si el usuario existe
                string queryUsuario = "SELECT COUNT(*) FROM (SELECT Email FROM Clientes UNION SELECT Email FROM Administradores) AS Usuarios WHERE Email = @Email";
                using (SqlCommand cmdUsuario = new SqlCommand(queryUsuario, con))
                {
                    cmdUsuario.Parameters.AddWithValue("@Email", email);
                    int usuarioExiste = (int)cmdUsuario.ExecuteScalar();

                    if (usuarioExiste == 0)
                    {
                        ViewBag.Error = "⚠️ El usuario no está registrado.";
                        return View();
                    }
                }

                // Verificar si es un Cliente
                string queryCliente = "SELECT ID, Nombre FROM Clientes WHERE Email = @Email AND Contraseña = @Password";
                using (SqlCommand cmdCliente = new SqlCommand(queryCliente, con))
                {
                    cmdCliente.Parameters.AddWithValue("@Email", email);
                    cmdCliente.Parameters.AddWithValue("@Password", hashedPassword);
                    using (SqlDataReader readerCliente = cmdCliente.ExecuteReader())
                    {
                        if (readerCliente.HasRows)
                        {
                            readerCliente.Read();  // 🔥 Importante: leer los datos antes de acceder
                            Session["Usuario"] = readerCliente["Nombre"].ToString();
                            Session["TipoUsuario"] = "Cliente";
                            Session["ClienteID"] = readerCliente["ID"].ToString();  // 🔥 Ahora sí se almacena bien
                            Console.WriteLine("📢 ClienteID guardado en sesión: " + Session["ClienteID"]); // Depuración
                            return RedirectToAction("Index", "Citas");
                        }
                    }
                }

                //  Verificar si es un Administrador
                string queryAdmin = "SELECT ID, Nombre FROM Administradores WHERE Email = @Email AND Contraseña = @Password";
                using (SqlCommand cmdAdmin = new SqlCommand(queryAdmin, con))
                {
                    cmdAdmin.Parameters.AddWithValue("@Email", email);
                    cmdAdmin.Parameters.AddWithValue("@Password", hashedPassword);
                    using (SqlDataReader readerAdmin = cmdAdmin.ExecuteReader())
                    {
                        if (readerAdmin.HasRows)
                        {
                            readerAdmin.Read();
                            Session["Usuario"] = readerAdmin["Nombre"].ToString();
                            Session["TipoUsuario"] = "Administrador";
                            return RedirectToAction("Index", "Citas"); // Administrador redirigido a listado de citas
                        }
                    }
                }
            }

            ViewBag.Error = "⚠️ Contraseña incorrecta.";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
