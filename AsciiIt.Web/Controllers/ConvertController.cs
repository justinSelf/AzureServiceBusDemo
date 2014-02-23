using System;
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

        [HttpPost]
        public FileResult Index(HttpPostedFileBase image)
        {
            return new FileStreamResult(image.InputStream, image.ContentType);
        }
    }
}