using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TallerMecanicoMVC.Controllers
{
    public class PruebaConexionController : Controller
    {
        public string Index()
        {
            string connStr = ConfigurationManager.ConnectionStrings["TallerMecanico"].ConnectionString;
            try
            {
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();
                    return "✅ Conexión exitosa a la base de datos";
                }
            }
            catch (Exception ex)
            {
                return "❌ Error de conexión: " + ex.Message;
            }
        }
    }
}