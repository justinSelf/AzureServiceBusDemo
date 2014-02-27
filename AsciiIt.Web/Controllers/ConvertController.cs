using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AsciiIt.Web.Services;

namespace AsciiIt.Web.Controllers
{
    public class ConvertController : Controller
    {
        public ActionResult Index(bool fileWasnotAnImage = false)
        {
            return View(fileWasnotAnImage);
        }

       
    }
}