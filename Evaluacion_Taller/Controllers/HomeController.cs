using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Evaluacion_Taller.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult DashboardCliente()
        {
            if (Session["Usuario"] == null || (string)Session["TipoUsuario"] != "Cliente")
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        public ActionResult DashboardAdmin()
        {
            if (Session["Usuario"] == null || (string)Session["TipoUsuario"] != "Administrador")
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }
    }

}