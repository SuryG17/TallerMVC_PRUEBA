using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
                int nuevoVehiculoID = 0;

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

                        // Parámetro de salida
                        SqlParameter outputParam = new SqlParameter("@NuevoVehiculoID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputParam);

                        cmd.ExecuteNonQuery();

                        nuevoVehiculoID = Convert.ToInt32(outputParam.Value);
                    }
                }

                if (nuevoVehiculoID == -1)
                {
                    return Json(new { success = false, message = "⚠️ Las placas ya están registradas en otro vehículo." });
                }

                return Json(new { success = true, idVehiculo = nuevoVehiculoID, nombreVehiculo = Marca + " - " + Modelo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ Error al agregar el vehículo: " + ex.Message });
            }
        }


        [HttpGet]
        public ActionResult Crear()
        {
            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ID, Marca + ' - ' + Modelo AS NombreVehiculo FROM Vehiculos", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                List<SelectListItem> vehiculos = new List<SelectListItem>();
                while (reader.Read())
                {
                    vehiculos.Add(new SelectListItem
                    {
                        Value = reader["ID"].ToString(),
                        Text = reader["NombreVehiculo"].ToString()
                    });
                }

                ViewBag.Vehiculos = new SelectList(vehiculos, "Value", "Text");
            }

            return View(); 
        }

        // 📌 Agendar Cita
        [HttpPost]
        public ActionResult Crear(Cita cita)
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

                        // Parámetros de entrada
                        cmd.Parameters.AddWithValue("@VehiculoID", cita.IdVehiculo);
                        cmd.Parameters.AddWithValue("@FechaHora", cita.FechaHora);

                        // Parámetro de salida
                        SqlParameter outputParam = new SqlParameter("@Resultado", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputParam);

                        // Ejecutar procedimiento almacenado
                        cmd.ExecuteNonQuery();

                        // Capturar el resultado
                        int resultado = (int)outputParam.Value;

                        switch (resultado)
                        {
                            case -1:
                                ViewBag.Error = "⚠️ El vehículo seleccionado no existe.";
                                return View("Crear", cita);

                            case 0:
                                ViewBag.Error = "⚠️ Esta hora ya está ocupada. Seleccione otra.";
                                return View("Crear", cita);

                            case 1:
                                ViewBag.Success = "✅ Cita agendada correctamente.";
                                return RedirectToAction("Index");

                            default:
                                ViewBag.Error = "⚠️ Ocurrió un error inesperado.";
                                return View("Crear", cita);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "❌ Error en la base de datos: " + ex.Message;
                    return View("Crear", cita);
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
