using System.Web;
using System.Web.Mvc;

namespace AsciiIt.Web.Controllers
{
    public class ConvertController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Index(HttpPostedFile image)
        {
            return Json("Success");
        }
    }
}