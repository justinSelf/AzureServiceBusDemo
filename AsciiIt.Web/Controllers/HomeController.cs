using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AsciiIt.Web.Models;
using AsciiIt.Web.Services;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;

namespace AsciiIt.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string message = "")
        {
            return View(new MessageModel { Message = message });
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase image)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.QueueExists("ImageProcessing"))
            {
                namespaceManager.CreateQueue("ImageProcessing");
            }

            var client = QueueClient.CreateFromConnectionString(connectionString, "ImageProcessing");

            client.Send(new BrokeredMessage());

            //if (image == null) return Index("Where's the beef?");
            //var imageStreamConverter = new ImageStreamConverter();
            //var bitmap = imageStreamConverter.GetBitmapFromPostedFile(image);

            //if (bitmap == null) return Index("That's not an image, homie...");

            //var asciiService = new AsciiImageCoverterService();

            //var stream = GetAsciiArtStream(asciiService, bitmap);
            //var headerValue = string.Format("attachment; filename={0}_converted.txt", image.FileName);

            //Response.AddHeader("Content-Disposition", headerValue);

            //return new FileStreamResult(stream, "application/text");
            return Index();
        }

        private static MemoryStream GetAsciiArtStream(AsciiImageCoverterService asciiService, Bitmap bitmap)
        {
            var convertedAsciiArt = asciiService.ConvertImage(bitmap);

            byte[] asciiArtBytes = Encoding.UTF8.GetBytes(convertedAsciiArt);
            var stream = new MemoryStream(asciiArtBytes);
            return stream;
        }
    }
}