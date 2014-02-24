using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web;

namespace AsciiIt.Web.Services
{
    public class ImageStreamConverter
    {
        private readonly List<string> imageExtensions = new List<string> { "jpeg", "jpg", "bmp", "png" };
        private bool FileHasImageExtension(string postedFileName)
        {
            var splitFileName = postedFileName.Split('.');

            var extension = splitFileName[splitFileName.Length - 1];

            return imageExtensions.Contains(extension);
        }

        public Bitmap GetBitmapFromPostedFile(HttpPostedFileBase postedFile)
        {
            if (!FileHasImageExtension(postedFile.FileName)) return null;

            try
            {
                var image = new Bitmap(postedFile.InputStream);
                return image;
            }
            catch (ArgumentException)
            {
                return null;
            }

        }
    }
}