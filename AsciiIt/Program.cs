using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;

namespace AsciiIt
{
    class Program
    {
        static void Main(string[] args)
        {
            var grayscales = GenerateGrayscaleMap();
            var normalizedGrayscales = NormalizeGrayscales(grayscales);

            var image = new Bitmap("cat.jpeg");

            int minGray;
            int maxGray;

            var grayscaleMatrix = GenerateGrayscaleMatrix(image, out maxGray, out minGray);

            var sb = new StringBuilder(256);
            sb.Append("<pre>");
            for (int j = 0; j < (image.Height); j++)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    var normalizedValue = 1 + (grayscaleMatrix[i, j] - minGray) * 255 / (maxGray - minGray);
                    var replaceCharacter = normalizedGrayscales.First(pair => pair.Key >= normalizedValue).Value;

                    sb.Append(replaceCharacter);
                }
                sb.AppendLine();
            }

            sb.Append(@"</pre>");

            var file = File.CreateText("test.html");

            file.Write(sb.ToString());

        }

        private static int[,] GenerateGrayscaleMatrix(Bitmap image, out int maxGray, out int minGray)
        {
            maxGray = int.MinValue;
            minGray = int.MaxValue;

            var grayscaleMatrix = new int[(image.Width), (image.Height)];

            for (int n = 0; n < image.Height; n++)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    var pixel = image.GetPixel(i, n);
                    var currentPixelGrayscale = (int)((pixel.R * .3) + (pixel.G * .59) + (pixel.B * .11));

                    if (currentPixelGrayscale > maxGray) maxGray = currentPixelGrayscale;
                    if (currentPixelGrayscale < minGray) minGray = currentPixelGrayscale;

                    grayscaleMatrix[i, n] = currentPixelGrayscale;
                }
            }
            return grayscaleMatrix;
        }

        private static List<KeyValuePair<int, char>> NormalizeGrayscales(SortedDictionary<int, char> grayscales)
        {
            var max = grayscales.Aggregate(int.MinValue, (memo, pair) => memo < pair.Key ? pair.Key : memo);
            var min = grayscales.Aggregate(int.MaxValue, (memo, pair) => memo > pair.Key ? pair.Key : memo);

            var range = max - min;

            Func<KeyValuePair<int, char>, KeyValuePair<int, char>> normalizePair = (pair) =>
            {
                var newKey = 1 + (pair.Key - min) * 255 / range;
                return new KeyValuePair<int, char>(newKey, pair.Value);
            };

            var normalizedGrayscales = grayscales.Select(normalizePair).ToList();
            return normalizedGrayscales;
        }

        private static SortedDictionary<int, char> GenerateGrayscaleMap()
        {
            var grayscales = new SortedDictionary<int, char>();

            for (int i = 33; i < 126; i++)
            {
                var characterImage = DrawText(((char)i).ToString(), new Font(FontFamily.GenericMonospace, 10), Color.Black,
                    Color.White);

                var grayscale = 0;

                for (int width = 0; width < characterImage.Width; width++)
                {
                    for (int height = 0; height < characterImage.Height; height++)
                    {
                        var pixel = (characterImage as Bitmap).GetPixel(width, height);
                        grayscale += (int)((pixel.R * .3) + (pixel.G * .59) + (pixel.B * .11));
                    }
                }

                grayscale = grayscale / (characterImage.Width * characterImage.Height);
                if (!grayscales.ContainsKey(grayscale)) grayscales.Add(grayscale, (char)i);
            }
            return grayscales;
        }

        /// <summary>
        /// This method is shamelessly lifted from this stackoverflow answer: http://stackoverflow.com/a/2070493/899048
        /// </summary>
        private static Image DrawText(String text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);
             
            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;

        }
    }
}
