using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SmartCafe.Interfaces;
namespace SmartCafe.Services
{
    public class FileService(IConfiguration configuration) :IFileService
    {
        public async Task<bool> WriteImageDocker(IFormFile file, string name, string folder)
        {
            try
            {
                #region Path Built

                string pathBuilt = Path.Combine(configuration.GetSection("Application_Path").Value ?? "", $"wwwroot/images/{folder}/");//build folder image store

                if (!Directory.Exists(pathBuilt))
                {
                    _ = Directory.CreateDirectory(pathBuilt);
                }

                #endregion

                string extension = ("." + file.FileName.Split('.')[^1]).ToLower();
                string path = pathBuilt + name + extension;

                bool result = await CompressAndSaveImage(file, path, 30);

                return result;
            }
            catch (Exception ex)
            {
                // logger.LogError("{message}", ex.Message);
                Console.WriteLine($"IMAGE SAVE ERROR: {ex.Message} -> {ex.InnerException?.Message}");
                return false;
            }
        }

        private async Task<bool> CompressAndSaveImage(IFormFile file, string outputPath, int quality)
        {
            try
            {
                if (file.Length == 0)
                    return false;

                string extension = Path.GetExtension(outputPath).ToLower(); // Get target file extension

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream); // Copy file to memory first
                memoryStream.Position = 0; // Reset position for reading

                using var image = await SixLabors.ImageSharp.Image.LoadAsync(memoryStream);
                await using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                // Determine the encoder based on the file extension
                IImageEncoder encoder = extension switch
                {
                    ".jpg" or ".jpeg" => new JpegEncoder { Quality = quality },
                    ".png" => new PngEncoder { CompressionLevel = PngCompressionLevel.DefaultCompression },
                    ".gif" => new GifEncoder(),
                    ".bmp" => new BmpEncoder(),
                    _ => throw new NotSupportedException($"The file extension '{extension}' is not supported."),
                };

                // Save the compressed image to the file
                await image.SaveAsync(outputStream, encoder);
                // logger.LogInformation("Image saved at: {outputPath}", outputPath);

                return true;
            }
            catch (Exception ex)
            {
                //logger.LogError("[Image Compress Error]: {message}", ex.Message);
                return false;
            }
        }
    }
}
