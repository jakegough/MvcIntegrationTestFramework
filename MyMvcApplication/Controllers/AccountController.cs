using System.Web.Mvc;
using System.Web.Security;

namespace MyMvcApplication.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOn(string username, string password)
        {
            if ((username == "steve") && (password == "secret"))
            {
                FormsAuthentication.RedirectFromLoginPage(username, false);
            }

            ModelState.AddModelError("username", "Either the username or password is incorrect.");
            return View();
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }
    }
}