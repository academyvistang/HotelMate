using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelMateWebV1.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        public ActionResult TimeoutRedirect()
        {
            return View();
        }

        public ActionResult CustomErrorView()
        {
            return View("Index");
        }

        public ActionResult Index()
        {
            return View("Index");
        }

        // Return the 404 not found page 
        public ActionResult Error404()
        {
            return View("Index");
        }

        // Return the 404 not found page 
        public ActionResult ErrorPage()
        {
            return View("Index");
        }

        // Return the 500 not found page 
        public ActionResult Error500()
        {
            return View("Index");
        }
    }
}