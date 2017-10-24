using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private WebApplication2.Models.DBContext db = new WebApplication2.Models.DBContext();
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult DBView()
        {
            ViewBag.Title = "DB";

            return View(db.Workers);
        }
    }
}
