using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AsciiIt.Contracts;
using AsciiIt.Web.Models;
using AsciiIt.Web.Services;
using Microsoft.Ajax.Utilities;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

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
            var serviceBusConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var blobConnectionString = CloudConfigurationManager.GetSetting("BlobStorage.ConnectionString");

            var storageAccount = CloudStorageAccount.Parse(blobConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("images");

            container.CreateIfNotExists();

            if (image == null) return Index("Where's the beef?");
            var imageStreamConverter = new ImageStreamConverter();
            var bitmap = imageStreamConverter.GetBitmapFromPostedFile(image);

            if (bitmap == null) return Index("That's not an image, homie...");

            var blockReference = container.GetBlockBlobReference(Guid.NewGuid().ToString());


            var converter = new ImageConverter();
            var bitmapBytes = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));

            blockReference.UploadFromByteArray(bitmapBytes, 0, bitmapBytes.Length);

            var namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);

            if (!namespaceManager.QueueExists("ImageProcessing"))
            {
                namespaceManager.CreateQueue("ImageProcessing");
            }

            var client = QueueClient.CreateFromConnectionString(serviceBusConnectionString, "ImageProcessing");
            var message = new BrokeredMessage(new ImageMessage { BlobBlockName = blockReference.Name });

            client.Send(message);

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