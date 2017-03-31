using System;
using System.Web;
using System.Web.Mvc;

namespace MyMvcApplication.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        [HttpGet]
        public ActionResult About()
        {
            return View();
        }

        public ActionResult DoStuffWithSessionAndCookies()
        {
            Session["myIncrementingSessionItem"] = ((int?) (Session["myIncrementingSessionItem"] ?? 0)) + 1;
            Response.Cookies.Add(new HttpCookie("mycookie", "myval"));
            return Content("OK");
        }

        public ActionResult FaultyRoute()
        {
            throw new NullReferenceException("This is a sample exception");
        }

        [Authorize]
        public ActionResult SecretAction()
        {
            return Content("Hello, you're logged in as " + User.Identity.Name);
        }
    }
}