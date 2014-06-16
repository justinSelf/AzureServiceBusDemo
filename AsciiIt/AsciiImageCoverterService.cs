using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;

namespace AsciiIt
{
    public class AsciiImageCoverterService
    {
        private List<KeyValuePair<int, char>> NormalizeGrayscales(SortedDictionary<int, char> grayscales)
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
        private List<KeyValuePair<int, char>> GenerateGrayscaleMap()
        {
            var grayscales = new SortedDictionary<int, char>();

            for (int i = 33; i < 126; i++)
            {
                var characterImage = CreateImageFromCharacter((char)i);

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
            return NormalizeGrayscales(grayscales);
        }

        /// <summary>
        /// This method is shamelessly lifted from this stackoverflow answer: http://stackoverflow.com/a/2070493/899048
        /// </summary>
        private static Image CreateImageFromCharacter(char character)
        {
            var font = new Font(FontFamily.GenericMonospace, 10);
            var text = character.ToString();
            var textColor = Color.Black;
            var backColor = Color.White;
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);
            SizeF textSize = drawing.MeasureString(text, font);

            img.Dispose();
            drawing.Dispose();

            img = new Bitmap((int)textSize.Width, (int)textSize.Height);
            drawing = Graphics.FromImage(img);
            drawing.Clear(backColor);

            Brush textBrush = new SolidBrush(textColor);
            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();
            textBrush.Dispose();
            drawing.Dispose();

            return img;
        }

        private static int[,] GenerateGrayscaleMatrix(Bitmap image, out int maxGray, out int minGray)
        {
            maxGray = int.MinValue;
            minGray = int.MaxValue;


            const int MAX_WIDTH = 300;

            int blockWidth = image.Width / MAX_WIDTH;

            if (blockWidth < 1) blockWidth = 1;

            int blockHeight = (blockWidth * 2);

            var grayscaleMatrix = new int[(image.Width / blockWidth), (image.Height / blockHeight)];

            var currentBlock = new List<int>();
            for (int n = 0; n < grayscaleMatrix.GetLength(1); n++)
            {
                for (int i = 0; i < grayscaleMatrix.GetLength(0); i++)
                {
                    for (int j = 0; j < blockHeight; j++)
                    {
                        for (int k = 0; k < blockWidth; k++)
                        {
                            var pixel = image.GetPixel((i * blockWidth) + k, (n * blockHeight) + j);
                            var currentPixelGrayscale = (int)((pixel.R * .3) + (pixel.G * .59) + (pixel.B * .11));

                            currentBlock.Add(currentPixelGrayscale);
                        }
                    }

                    var averageGrayscale = currentBlock.Aggregate((memo, currentValue) => memo + currentValue) / currentBlock.Count;
                    currentBlock.Clear();
                    if (averageGrayscale > maxGray) maxGray = averageGrayscale;
                    if (averageGrayscale < minGray) minGray = averageGrayscale;

                    grayscaleMatrix[i, n] = averageGrayscale;

                }
            }
            return grayscaleMatrix;
        }

        public string ConvertImage(Bitmap image)
        {
            int minGray;
            int maxGray;

            var grayscaleMatrix = GenerateGrayscaleMatrix(image, out maxGray, out minGray);
            var normalizedGrayscales = GenerateGrayscaleMap();

            var sb = new StringBuilder(256);
            for (int j = 0; j < (grayscaleMatrix.GetLength(1)); j++)
            {
                for (int i = 0; i < grayscaleMatrix.GetLength(0); i++)
                {
                    var normalizedValue = 1 + (grayscaleMatrix[i, j] - minGray) * 255 / (maxGray - minGray);
                    var replaceCharacter = normalizedGrayscales.First(pair => pair.Key >= normalizedValue).Value;

                    sb.Append(replaceCharacter);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
