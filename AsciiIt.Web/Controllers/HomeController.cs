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
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;

namespace AsciiIt.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase image)
        {
            if (image == null) return Index();
            var imageStreamConverter = new ImageStreamConverter();
            var bitmap = imageStreamConverter.GetBitmapFromPostedFile(image);

            if (bitmap == null) return Index();

            var asciiService = new AsciiImageCoverterService();

            var stream = GetAsciiArtStream(asciiService, bitmap);
            var headerValue = string.Format("attachment; filename={0}_converted.txt", image.FileName);

            Response.AddHeader("Content-Disposition", headerValue);

            return new FileStreamResult(stream, "application/text");
        }


        public ActionResult Gallery()
        {
            var blobConnectionString = CloudConfigurationManager.GetSetting("BlobStorage.ConnectionString");

            var storageAccount = CloudStorageAccount.Parse(blobConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("converted-images");
            container.CreateIfNotExists();

            var convertedImageBlobs = container.ListBlobs();

            return View(convertedImageBlobs);
        }

        public ActionResult ConvertedImage(string name)
        {
            var blobConnectionString = CloudConfigurationManager.GetSetting("BlobStorage.ConnectionString");

            var storageAccount = CloudStorageAccount.Parse(blobConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("converted-images");
            container.CreateIfNotExists();

            var blob = container.GetBlockBlobReference(name);
            var asciiImage = blob.DownloadText();

            var model = new AsciiImage { ImageText = asciiImage, Name = name };

            return View(model);
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