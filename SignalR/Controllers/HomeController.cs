using System.Web.Mvc;

namespace SignalR.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult SignalR()
        {
            ViewBag.Message = "Showing SignalR in motion.";

            return View();
        }
    }
}