using System.Web.Mvc;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// Main page controller.
    /// </summary>
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return User.Identity.IsAuthenticated ? View("Dashboard") : View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}
