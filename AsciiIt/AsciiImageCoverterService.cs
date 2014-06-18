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

        private static int[,] GenerateGrayscaleMatrix(Bitmap image, out int maxGray, out int minGray, int outputWidth = 80)
        {
            maxGray = int.MinValue;
            minGray = int.MaxValue;
            int outputHeight = (int)((decimal)outputWidth/((decimal)image.Width/((decimal)image.Height)));

            System.Diagnostics.Debug.WriteLine("Original dimensions {0}x{1}", new object[] { image.Width, image.Height });
            System.Diagnostics.Debug.WriteLine("Scaled dimensions {0}x{1}", new object[] { outputWidth, outputHeight });

            var grayscaleMatrix = new int[outputWidth, outputHeight];

            decimal horizontalStep = image.Width/outputWidth;
            decimal verticalStep = horizontalStep;

            var averageGrayscalePerBlock = new List<int>();
            int currentPixelGrayscale = 0;

            for (int y = 0; y < outputHeight; y++)
            {
                for (int x = 0; x < outputWidth; x++)
                {
                    averageGrayscalePerBlock.Clear();

                    // Average the block of pixels
                    for (int n = (int)((decimal)y * verticalStep); n < Math.Min((int)((decimal)(y + 1) * verticalStep), image.Height); n++)
                    {
                        for (int i = (int)((decimal)x * horizontalStep); i < Math.Min((int)((decimal)(x + 1) * horizontalStep), image.Width); i++)
                        {
                            System.Diagnostics.Debug.WriteLine("x,y = {0},{1}; i,n = {2},{3}", new object[] { x, y, i, n } );
                            var pixel = image.GetPixel(i, n);
                            averageGrayscalePerBlock.Add((int)((pixel.R * .3) + (pixel.G * .59) + (pixel.B * .11)));
                        }
                    }
                    if (averageGrayscalePerBlock.Count == 0)
                        continue;

                    currentPixelGrayscale = (int)averageGrayscalePerBlock.Average();
                    grayscaleMatrix[x, y] = currentPixelGrayscale;
                    if (currentPixelGrayscale > maxGray) maxGray = currentPixelGrayscale;
                    if (currentPixelGrayscale < minGray) minGray = currentPixelGrayscale;
                    averageGrayscalePerBlock.Clear();

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
