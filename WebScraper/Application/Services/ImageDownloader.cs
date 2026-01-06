using System.Net;

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

                // Generate a unique file name
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(saveDirectory, fileName);

                // Download the image
                using var response = await _httpClient.GetAsync(imageUrl);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    await response.Content.CopyToAsync(fileStream);
                }

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