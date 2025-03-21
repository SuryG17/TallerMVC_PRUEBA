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
            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_TM_Seleccionar_Citas", conn);
                cmd.CommandType = CommandType.StoredProcedure;

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
                // 🔍 Verificar si ClienteID está en la sesión
                if (ClienteID == 0 )
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
            List<SelectListItem> vehiculos = new List<SelectListItem>();
            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT ID, Marca + ' - ' + Modelo AS NombreVehiculo FROM Vehiculos", conn
                );
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    vehiculos.Add(new SelectListItem
                    {
                        Value = reader["ID"].ToString(),
                        Text = reader["NombreVehiculo"].ToString()
                    });
                }
            }
           
            ViewBag.Vehiculos = new SelectList(vehiculos, "Value", "Text");

            // Retorna el modelo vacío o con valores por defecto
            return View(new Cita());
        }




        // 📌 Agendar Cita
        [HttpPost]
        public ActionResult AgendarCita(Cita cita)
        {
            {
                if (cita == null || cita.IdVehiculo <= 0 || cita.FechaHora == DateTime.MinValue)
                {
                    ViewBag.Error = "⚠️ Datos inválidos. Verifique la información ingresada.";
                    return View("Crear", cita);
                }

                using (SqlConnection conn = new SqlConnection(conexion))
                {
                    try
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand("SP_TM_AgendarCita", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@VehiculoID", cita.IdVehiculo);
                            cmd.Parameters.AddWithValue("@FechaHora", cita.FechaHora);

                            SqlParameter outputParam = new SqlParameter("@Resultado", SqlDbType.Int)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(outputParam);

                            cmd.ExecuteNonQuery();

                            int resultado = Convert.ToInt32(outputParam.Value);

                            if (resultado == -1)
                            {
                                ViewBag.Error = "⚠️ El vehículo seleccionado no pertenece al cliente.";
                                return View("Crear", cita);
                            }

                            if (resultado == -2)
                            {
                                ViewBag.Error = "⚠️ Ya tenemos una cita programada a esa hora, Por favor seleccione otra y con gusto lo atenderemos.";
                                return View("Crear", cita);
                            }

                            ViewBag.Success = "✅ Cita agendada correctamente.";
                            return RedirectToAction("Index");
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = "❌ Error en la base de datos: " + ex.Message;
                        return View("Crear", cita);
                    }
                }
            }
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


    }
}
