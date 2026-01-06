using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;

namespace Application.Services
{
    public class ImageDownloader
    {
        private readonly HttpClient _httpClient;

        public ImageDownloader(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> DownloadImageAsync(string imageUrl, string saveDirectory)
        {
            try
            {
                // Ensure the save directory exists
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                // Download the image
                using var response = await _httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                // Determine a safe file extension
                string? fileNameFromUrl = null;
                try
                {
                    var uri = new Uri(imageUrl);
                    fileNameFromUrl = Path.GetFileName(uri.LocalPath);
                }
                catch
                {
                    // ignore
                }

                var ext = Path.GetExtension(fileNameFromUrl ?? string.Empty);
                if (string.IsNullOrEmpty(ext))
                {
                    var media = response.Content.Headers.ContentType?.MediaType;
                    if (!string.IsNullOrEmpty(media) && media.StartsWith("image/"))
                    {
                        var subtype = media.Substring("image/".Length);
                        if (subtype.Equals("jpeg", StringComparison.InvariantCultureIgnoreCase)) subtype = "jpg";
                        // remove any +suffix (e.g. image/svg+xml)
                        var plus = subtype.IndexOf('+');
                        if (plus > 0) subtype = subtype.Substring(0, plus);
                        ext = "." + subtype;
                    }
                    else
                    {
                        ext = ".jpg"; // fallback
                    }
                }

                // Create a safe file name
                var namePart = Path.GetFileNameWithoutExtension(fileNameFromUrl ?? "image");
                if (string.IsNullOrEmpty(namePart)) namePart = "image";
                // sanitize: replace invalid filename chars with underscore
                namePart = Regex.Replace(namePart, "[^A-Za-z0-9_\\-]", "_");
                var fileName = $"{namePart}_{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(saveDirectory, fileName);

                await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await response.Content.CopyToAsync(fileStream);

                return filePath;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new InvalidOperationException($"Failed to download image from {imageUrl}", ex);
            }
        }
    }
}