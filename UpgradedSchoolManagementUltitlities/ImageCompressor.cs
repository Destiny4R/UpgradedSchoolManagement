using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace UpgradedSchoolManagementUltitlities
{
    public static class ImageCompressor
    {
        public static async Task<string> CompressAndSaveImageAsync(
    IFormFile file,
    string webRootPath,
    string subFolder = "uploads/signatures")
        {
            var folder = Path.Combine(webRootPath, subFolder);
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var isPng = ext == ".png";

            var fileName = $"{Guid.NewGuid()}.jpg";
            var relativePath = $"/{subFolder}/{fileName}".Replace("\\", "/");
            var fullPath = Path.Combine(folder, fileName);

            using var inputStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(inputStream);

            if (isPng)
            {
                using var ms = new MemoryStream();
                await image.SaveAsJpegAsync(ms, new JpegEncoder { Quality = 90 });

                var data = ms.ToArray();

                if (data.Length >= 100 * 1024 && data.Length <= 200 * 1024)
                {
                    await System.IO.File.WriteAllBytesAsync(fullPath, data);
                    return relativePath;
                }

                using var byteStream = new MemoryStream(data);
                using var reloaded = await Image.LoadAsync(byteStream);

                await CompressToTargetRangeAsync(reloaded, fullPath);
                return relativePath;
            }

            await CompressToTargetRangeAsync(image, fullPath);
            return relativePath;
        }

        public static async Task CompressToTargetRangeAsync(Image image, string fullPath)
        {
            const long minBytes = 100 * 1024;
            const long maxBytes = 200 * 1024;

            int quality = 90;
            byte[] compressed;

            do
            {
                using var ms = new MemoryStream();
                await image.SaveAsJpegAsync(ms, new JpegEncoder { Quality = quality });
                compressed = ms.ToArray();
                quality -= 5;
            }
            while (compressed.Length > maxBytes && quality >= 15);

            if (compressed.Length < minBytes && quality < 95)
            {
                quality = Math.Min(quality + 10, 95);
                using var ms = new MemoryStream();
                await image.SaveAsJpegAsync(ms, new JpegEncoder { Quality = quality });
                compressed = ms.ToArray();
            }

            await System.IO.File.WriteAllBytesAsync(fullPath, compressed);
        }
    }
}
