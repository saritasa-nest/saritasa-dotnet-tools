using System.Web.Mvc;

namespace Saritasa.BoringWarehouse.Web.Controllers
{
    /// <summary>
    /// Index.
    /// </summary>
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}
