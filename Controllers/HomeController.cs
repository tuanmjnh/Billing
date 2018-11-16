using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Billing.Controllers
{
    [Filters.Auth]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.data = Server.MapPath("~/");
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}