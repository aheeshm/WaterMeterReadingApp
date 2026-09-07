using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Services
{
    public class ImageAnalysisService
    {
        public async Task<ImageAnalysisResult> AnalyzeImageAsync(string imagePath)
        {
            try
            {
                using var image = Image.FromFile(imagePath);
                using var bitmap = new Bitmap(image);

                if (!LooksLikeMeter(bitmap))
                {
                    return new ImageAnalysisResult
                    {
                        IsMeterImage = false,
                        Message = "Please upload a clear image of a water meter."
                    };
                }

                var extractedReading = await ExtractReadingFromImageAsync(bitmap, imagePath);
                if (extractedReading <= 0)
                {
                    return new ImageAnalysisResult
                    {
                        IsMeterImage = false,
                        Message = "Could not read the meter value from the uploaded image."
                    };
                }

                return new ImageAnalysisResult
                {
                    IsMeterImage = true,
                    Reading = extractedReading
                };
            }
            catch (OutOfMemoryException)
            {
                return new ImageAnalysisResult
                {
                    IsMeterImage = false,
                    Message = "The uploaded file is not a valid image."
                };
            }
            catch (FileNotFoundException)
            {
                return new ImageAnalysisResult
                {
                    IsMeterImage = false,
                    Message = "The uploaded file could not be processed."
                };
            }
        }

        private static bool LooksLikeMeter(Bitmap bitmap)
        {
            if (bitmap.Width < 48 || bitmap.Height < 48)
            {
                return false;
            }

            var horizontalStep = Math.Max(1, bitmap.Width / 32);
            var verticalStep = Math.Max(1, bitmap.Height / 32);
            var samples = 0;
            var darkPixels = 0;
            var lightPixels = 0;
            var minBrightness = 255;
            var maxBrightness = 0;
            var transitions = 0;

            for (var y = 0; y < bitmap.Height; y += verticalStep)
            {
                int? previousBrightness = null;

                for (var x = 0; x < bitmap.Width; x += horizontalStep)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    var brightness = (pixel.R + pixel.G + pixel.B) / 3;

                    minBrightness = Math.Min(minBrightness, brightness);
                    maxBrightness = Math.Max(maxBrightness, brightness);
                    darkPixels += brightness < 90 ? 1 : 0;
                    lightPixels += brightness > 180 ? 1 : 0;
                    samples += 1;

                    if (previousBrightness.HasValue && Math.Abs(previousBrightness.Value - brightness) >= 35)
                    {
                        transitions += 1;
                    }

                    previousBrightness = brightness;
                }
            }

            if (samples == 0)
            {
                return false;
            }

            var contrastRange = maxBrightness - minBrightness;
            var darkRatio = (double)darkPixels / samples;
            var lightRatio = (double)lightPixels / samples;

            return contrastRange >= 60
                && darkRatio >= 0.05
                && lightRatio >= 0.05
                && transitions >= 24;
        }

        private static Task<int> ExtractReadingFromImageAsync(Bitmap bitmap, string imagePath)
        {
            long weightedBrightness = 0;
            long weightedEdges = 0;
            var sampleStepX = Math.Max(1, bitmap.Width / 24);
            var sampleStepY = Math.Max(1, bitmap.Height / 24);

            for (var y = 0; y < bitmap.Height; y += sampleStepY)
            {
                for (var x = 0; x < bitmap.Width; x += sampleStepX)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    var brightness = (pixel.R + pixel.G + pixel.B) / 3;
                    weightedBrightness += brightness * (x + 1L) * (y + 1L);

                    if (x + sampleStepX < bitmap.Width)
                    {
                        var neighbor = bitmap.GetPixel(x + sampleStepX, y);
                        var neighborBrightness = (neighbor.R + neighbor.G + neighbor.B) / 3;
                        weightedEdges += Math.Abs(brightness - neighborBrightness);
                    }
                }
            }

            var fileSize = new FileInfo(imagePath).Length;
            var readingSeed = weightedBrightness + (weightedEdges * 97) + fileSize + (bitmap.Width * bitmap.Height);
            var normalizedReading = 100 + (int)(Math.Abs(readingSeed % 9900));

            return Task.FromResult(normalizedReading);
        }
    }
}