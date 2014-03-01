using System.Drawing;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AsciiIt.Contracts;
using AsciiIt.Web.Models;
using AsciiIt.Web.Services;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AsciiIt.Web.Controllers
{
    public class HomeController : Controller
    {
        private const string CONVERTED_IMAGES_CONTAINER = "converted-images";
        private const string IMAGES_CONTAINER = "images";

        public ActionResult Index(string message = "")
        {
            return View(new MessageModel { Message = message });
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase image)
        {
            if (image == null) return Index("Where's the beef?");
            var imageStreamConverter = new ImageStreamConverter();
            var bitmap = imageStreamConverter.GetBitmapFromPostedFile(image);

            if (bitmap == null) return Index("That's not an image, homie...");


            var serviceBusConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var blobConnectionString = CloudConfigurationManager.GetSetting("BlobStorage.ConnectionString");

            var storageAccount = CloudStorageAccount.Parse(blobConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(IMAGES_CONTAINER);
            container.CreateIfNotExists();

            var blockReference = container.GetBlockBlobReference(image.FileName);
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
            return Index();
        }

        public ActionResult Gallery()
        {
            var container = GetBlobContainer(CONVERTED_IMAGES_CONTAINER);

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
            var container = GetBlobContainer(CONVERTED_IMAGES_CONTAINER);
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