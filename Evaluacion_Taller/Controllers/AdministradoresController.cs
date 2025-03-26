using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using TallerMecanicoMVC.Models;

namespace TallerMecanicoMVC.Controllers
{
    public class AdministradoresController : Controller
    {
        private string conexion = ConfigurationManager.ConnectionStrings["TallerMecanico"].ConnectionString;
        public ActionResult Index()
        {
            List<Cita> citas = new List<Cita>();

            if (Session["UsuarioID"] == null || Session["TipoUsuario"]?.ToString() != "Administrador")
            {
                ViewBag.Error = "⚠️ Acceso no autorizado.";
                return RedirectToAction("Index", "Login");
            }

            int usuarioID = Convert.ToInt32(Session["UsuarioID"]);

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_TM_Seleccionar_Citas", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UsuarioID", usuarioID);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    citas.Add(new Cita
                    {
                        IdCita = Convert.ToInt32(reader["ID"]),
                        Marca = reader["Marca"].ToString(),
                        Modelo = reader["Modelo"].ToString(),
                        Placas = reader["Placas"].ToString(),
                        FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                        Estado = reader["Estado"].ToString(),
                        Comentarios = reader["Comentarios"].ToString()
                    });
                }
            }

            return View(citas);
        }


        public ActionResult ListadoUsuarios()
        {
            List<Cliente> clientes = new List<Cliente>();

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_TM_ListarClientes", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    clientes.Add(new Cliente
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        Nombre = reader["Nombre"].ToString(),
                        Apellido = reader["Apellido"].ToString(),
                        Email = reader["Email"].ToString(),
                        Telefono = reader["Telefono"].ToString(),
                        FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"]),
                        Rol = reader["Rol"].ToString()
                    });
                }
            }

            return View(clientes);
        }

        [HttpGet]
        public ActionResult EditarRol(int id)
        {
            Cliente cliente = null;

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ID, Nombre, Apellido, Rol FROM Clientes WHERE ID = @ID", conn);
                cmd.Parameters.AddWithValue("@ID", id);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        cliente = new Cliente
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            Nombre = reader["Nombre"].ToString(),
                            Apellido = reader["Apellido"].ToString(),
                            Rol = reader["Rol"].ToString()
                        };
                    }
                }
            }

            if (cliente == null)
                return HttpNotFound();

            return View(cliente);
        }

        [HttpPost]
        public ActionResult EditarRol(int id, string nuevoRol)
        {
            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_TM_ActualizarRolCliente", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ClienteID", id);
                cmd.Parameters.AddWithValue("@NuevoRol", nuevoRol);

                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "✅ Rol actualizado correctamente.";
            return RedirectToAction("ListadoUsuarios");
        }


        public ActionResult EliminarUsuario(int id)
        {
            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_TM_EliminarCliente", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ClienteID", id);
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "✅ Cliente eliminado correctamente junto con sus citas y vehículos.";
            return RedirectToAction("ListadoUsuarios");
        }

        [HttpPost]
        public ActionResult EditarEstadoCita(int id, string estado, string comentario, DateTime? fechaTerminacion)
        {
            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_TM_Aprovar_Rechazar_Cita", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ID", id);
                cmd.Parameters.AddWithValue("@Estado", estado);
                cmd.Parameters.AddWithValue("@Comentarios", comentario);
                cmd.Parameters.AddWithValue("@FechaTerminacion", fechaTerminacion ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "✅ Cita actualizada correctamente.";
            return RedirectToAction("Index"); // Redirige a la vista de citas del admin
        }



    }
}
