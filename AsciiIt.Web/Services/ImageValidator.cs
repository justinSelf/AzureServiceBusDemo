using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web;

namespace AsciiIt.Web.Services
{
    public class ImageValidator
    {
        private readonly List<string> imageExtensions = new List<string> { "jpeg", "jpg", "bmp", "png" };
        public bool IsValidPostedImage(HttpPostedFileBase postedFile)
        {
            var splitFileName = postedFile.FileName.Split('.');

            var extension = splitFileName[splitFileName.Length - 1];

            return imageExtensions.Contains(extension);
        }
    }
}