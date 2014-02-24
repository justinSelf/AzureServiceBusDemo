using System;
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

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase image)
        {
            var imageStreamConverter = new ImageStreamConverter();
            var bitmap = imageStreamConverter.GetBitmapFromPostedFile(image);

            if (bitmap == null) return Index(true);

            var asciiService = new AsciiImageCoverterService();

            var convertedAsciiArt = asciiService.ConvertImage(bitmap);

            byte[] asciiArtBytes = Encoding.UTF8.GetBytes(convertedAsciiArt);
            var stream = new MemoryStream(asciiArtBytes);

            var convertedName = image.FileName + "_converted.txt";

            Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}", convertedName));

            return new FileStreamResult(stream, "application/text");
        }
    }
}