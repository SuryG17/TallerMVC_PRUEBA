using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using TallerMecanicoMVC.Models;

namespace TallerMecanicoMVC.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "⚠️ Por favor, completa todos los campos.";
                return View();
            }

            string hashedPassword = Cliente.EncriptarMD5(password);

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["TallerMecanico"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SP_TM_ValidarUsuario", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", hashedPassword);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        string tipoUsuario = reader["Rol"].ToString();

                        Session["Usuario"] = reader["Nombre"].ToString();
                        Session["TipoUsuario"] = tipoUsuario;
                        Session["UsuarioID"] = reader["ID"].ToString();

                        if (tipoUsuario == "Cliente")
                        {
                            Session["ClienteID"] = reader["ID"].ToString();
                            return RedirectToAction("Index", "Citas");
                        }
                        else if (tipoUsuario == "Administrador")
                        {
                            return RedirectToAction("Index", "Administradores");
                        }
                    }
                   
                }
            }

       
            ViewBag.Error = "❌ Usuario o contraseña incorrectos..";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
