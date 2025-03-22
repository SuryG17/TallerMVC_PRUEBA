using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using TallerMecanicoMVC.Models;

namespace TallerMecanicoMVC.Controllers
{
    public class CitasController : Controller
    {
        private string conexion = ConfigurationManager.ConnectionStrings["TallerMecanico"].ConnectionString;

        // 📌 Listar Citas
        public ActionResult Index()
        {
            List<Cita> citas = new List<Cita>();

            // Verificamos si el usuario está autenticado y obtenemos su ID y tipo desde la sesión
            int usuarioID = Convert.ToInt32(Session["UsuarioID"]);
            string tipoUsuario = Session["TipoUsuario"]?.ToString();

            if (usuarioID == 0 || string.IsNullOrEmpty(tipoUsuario))
            {
                ViewBag.Error = "⚠️ No se ha identificado al usuario.";
                return RedirectToAction("Index", "Login");
            }

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();

                // Llamamos al procedimiento almacenado SP_TM_Seleccionar_Citas y le pasamos el UsuarioID
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
        [HttpPost]
        public JsonResult AgregarVehiculo(int ClienteID, string Marca, string Modelo, int Año, string Placas)
        {
            try
            {
                // Verificar si ClienteID está en la sesión
                if (ClienteID == 0)
                {
                    ClienteID = Convert.ToInt32(Session["ClienteID"]);
                }

                Console.WriteLine($"📢 ClienteID en AgregarVehiculo: {ClienteID}");

                if (ClienteID == 0)
                {
                    return Json(new { success = false, message = "⚠️ ClienteID es inválido. Verifica si el usuario ha iniciado sesión." });
                }

                using (SqlConnection conn = new SqlConnection(conexion))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SP_TM_AgregarVehiculo", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClienteID", ClienteID);
                        cmd.Parameters.AddWithValue("@Marca", Marca);
                        cmd.Parameters.AddWithValue("@Modelo", Modelo);
                        cmd.Parameters.AddWithValue("@Año", Año);
                        cmd.Parameters.AddWithValue("@Placas", Placas);

                        SqlParameter outputParam = new SqlParameter("@NuevoVehiculoID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputParam);

                        cmd.ExecuteNonQuery();

                        int nuevoVehiculoID = Convert.ToInt32(outputParam.Value);

                        if (nuevoVehiculoID == -1)
                        {
                            return Json(new { success = false, message = "⚠️ Las placas ya están registradas en otro vehículo." });
                        }

                        if (nuevoVehiculoID == -2)
                        {
                            return Json(new { success = false, message = "⚠️ ClienteID no válido." });
                        }

                        return Json(new { success = true, idVehiculo = nuevoVehiculoID, nombreVehiculo = Marca + " - " + Modelo });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en AgregarVehiculo: " + ex.Message);
                return Json(new { success = false, message = "❌ Error en el servidor: " + ex.Message });
            }
        }



        [HttpGet]
        public ActionResult Crear()
        {
            var model = new CitaViewModel
            {
                Vehiculos = ObtenerVehiculosDelCliente() 
            };

            return View(model);
        }





        // 📌 Agendar Cita
        [HttpPost]
     
        public ActionResult AgendarCita(CitaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Vehiculos = ObtenerVehiculosDelCliente();
                return View("Crear", model);
            }

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SP_TM_AgendarCita", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VehiculoID", model.IdVehiculo);
                        cmd.Parameters.AddWithValue("@FechaHora", model.FechaHora);

                        SqlParameter outputParam = new SqlParameter("@Resultado", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputParam);

                        cmd.ExecuteNonQuery();

                        int resultado = Convert.ToInt32(outputParam.Value);

                        switch (resultado)
                        {
                            case -1:
                                ViewBag.Error = "🚫 El vehículo no pertenece a un cliente.";
                                break;
                            case -2:
                                ViewBag.Error = "📅 Ya existe una cita en ese rango.";
                                break;
                            case -3:
                                ViewBag.Error = "⏳ No puedes agendar en el pasado.";
                                break;
                            case -4:
                                ViewBag.Error = "🚗 El vehículo ya tiene una cita futura.";
                                break;
                            default:
                                TempData["Success"] = "✅ Cita agendada correctamente.";
                                return RedirectToAction("Index");
                        }

                        model.Vehiculos = ObtenerVehiculosDelCliente();
                        return View("Crear", model);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "❌ Error: " + ex.Message;
                    model.Vehiculos = ObtenerVehiculosDelCliente();
                    return View("Crear", model);
                }
            }
        }


        private IEnumerable<SelectListItem> ObtenerVehiculosDelCliente()
        {
            var lista = new List<SelectListItem>();

            using (var conn = new SqlConnection(conexion))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT ID, Marca + ' ' + Modelo + ' (' + Placas + ')' AS Nombre FROM Vehiculos WHERE ClienteID = @ClienteID", conn))
                {
                    cmd.Parameters.AddWithValue("@ClienteID", Session["ClienteID"]);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new SelectListItem
                            {
                                Value = reader["ID"].ToString(),
                                Text = reader["Nombre"].ToString()
                            });
                        }
                    }
                }
            }

            return lista;
        }

        private string ObtenerNombreVehiculo(int vehiculoID)
        {
            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Marca + ' - ' + Modelo FROM Vehiculos WHERE ID = @ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", vehiculoID);
                    object resultado = cmd.ExecuteScalar();
                    return resultado != null ? resultado.ToString() : "Desconocido";
                }
            }
        }



        [HttpGet]
        public ActionResult VerDetalles(int id)
        {
            Cita cita = null;

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(@"
            SELECT c.ID, c.FechaHora, c.Estado, c.Comentarios, c.VehiculoID, 
                   v.Marca + ' - ' + v.Modelo AS NombreVehiculo
            FROM Citas c
            INNER JOIN Vehiculos v ON c.VehiculoID = v.ID
            WHERE c.ID = @IdCita", conn))
                {
                    cmd.Parameters.AddWithValue("@IdCita", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cita = new Cita
                            {
                                IdCita = Convert.ToInt32(reader["ID"]),
                                FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                                Estado = reader["Estado"].ToString(),
                                Comentarios = reader["Comentarios"].ToString(),
                                IdVehiculo = Convert.ToInt32(reader["VehiculoID"]),
                                NombreVehiculo = reader["NombreVehiculo"].ToString()
                            };
                        }
                    }
                }
            }

            if (cita == null)
            {
                return HttpNotFound();
            }

            return View(cita);
        }




        [HttpPost]
        public ActionResult ActualizarCita(Cita cita)
        {
            if (cita == null || cita.FechaHora == DateTime.MinValue)
            {
                cita.NombreVehiculo = ObtenerNombreVehiculo(cita.IdVehiculo); 
                ViewBag.Error = "⚠️ Datos inválidos. Verifique la información ingresada.";
                return View("VerDetalles", cita);
            }

            int resultado = 0;
            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SP_TM_ActualizarHoraCita", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdCita", cita.IdCita);
                    cmd.Parameters.AddWithValue("@NuevaFechaHora", cita.FechaHora);

                    SqlParameter outputParam = new SqlParameter("@Resultado", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    cmd.ExecuteNonQuery();
                    resultado = Convert.ToInt32(outputParam.Value);
                }
            }

            if (resultado <= 0)
            {
                cita.NombreVehiculo = ObtenerNombreVehiculo(cita.IdVehiculo); 
                switch (resultado)
                {
                    case -1:
                        ViewBag.Error = "⚠️ La cita no existe.";
                        break;
                    case -2:
                        ViewBag.Error = "⚠️ Ya existe una cita en ese horario.";
                        break;
                    case -4:
                        ViewBag.Error = "⚠️ No puedes seleccionar una fecha anterior a la actual.";
                        break;
                    default:
                        ViewBag.Error = "❌ Error desconocido.";
                        break;
                }
                return View("VerDetalles", cita);
            }

            ViewBag.Success = "✅ Cita actualizada correctamente.";
            return RedirectToAction("Index");
        }







        // 📌 Aprobar/Rechazar Cita
        [HttpPost]
        public ActionResult AprobarRechazar(int id, string estado, string comentario, DateTime? fechaTerminacion)
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
            return RedirectToAction("Index");
        }

        public ActionResult CerrarSesion()
        {
            Session.Clear();

            
            return RedirectToAction("Index", "Login");
        }


    }
}
