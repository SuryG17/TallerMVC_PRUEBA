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

        // 📌 Agendar Cita
        [HttpPost]
        public ActionResult Agendar(Cita cita)
        {
            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SP_TM_AgendarCita", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@VehiculoID", cita.IdVehiculo);
                cmd.Parameters.AddWithValue("@FechaHora", cita.FechaHora);

                SqlParameter outputParam = new SqlParameter("@Resultado", SqlDbType.Int);
                outputParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(outputParam);

                cmd.ExecuteNonQuery();

                int resultado = (int)outputParam.Value;

                if (resultado == 0)
                {
                    ViewBag.Error = "⚠️ Esta hora ya está ocupada. Selecciona otra.";
                    return View("Agendar", cita);
                }
            }
            ViewBag.Success = "✅ Cita agendada correctamente.";
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
    }
}
