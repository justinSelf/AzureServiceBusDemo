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
using Microsoft.WindowsAzure.Storage.Blob;

namespace AsciiIt.Web.Controllers
{
    public class HomeController : Controller
    {
        private const string CONVERTED_CONTAINER = "converted-images";

        public ActionResult Index(string message = "")
        {
            return View((object)message);
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase image)
        {
            if (image == null) return Index("Looks like you forgot to pick an image.");

            var imageStreamConverter = new ImageStreamConverter();
            var bitmap = imageStreamConverter.GetBitmapFromPostedFile(image);

            if (bitmap == null) return Index("That's not an image, homie...");

            var container = GetBlobContainer(CONVERTED_CONTAINER);

            var blob = container.GetBlockBlobReference(image.FileName);
            //if (blob.Exists())
            //{
            //    return View((object)"An image with this name already exists in the gallery. Make sure the image has a unique name");
            //}

            var asciiService = new AsciiImageCoverterService();

            var stream = GetAsciiArtStream(asciiService, bitmap);

            blob.UploadFromStream(stream);

            return Index();
        }

        public ActionResult Gallery()
        {
            var container = GetBlobContainer(CONVERTED_CONTAINER);
            var convertedImageBlobs = container.ListBlobs();

            return View(convertedImageBlobs);
        }

        private static CloudBlobContainer GetBlobContainer(string containerName)
        {
            var blobConnectionString = CloudConfigurationManager.GetSetting("BlobStorage.ConnectionString");

            var storageAccount = CloudStorageAccount.Parse(blobConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            return container;
        }

        public ActionResult ConvertedImage(string name)
        {
            var container = GetBlobContainer(CONVERTED_CONTAINER);

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