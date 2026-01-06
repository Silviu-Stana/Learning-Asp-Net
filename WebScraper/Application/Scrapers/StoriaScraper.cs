using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Scrapers
{
    public class StoriaScraper : IScraper
    {
        private readonly ImageDownloader _imageDownloader;
        private readonly IHttpClientFactory _httpClientFactory;

        public StoriaScraper(ImageDownloader imageDownloader, IHttpClientFactory httpClientFactory)
        {
            _imageDownloader = imageDownloader;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Listing> ScrapeListingAsync(string url)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");

            var html = await client.GetStringAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            // Extract title
            var title = document.DocumentNode.SelectSingleNode("//h1[@data-cy='adPageAdTitle']")?.InnerText.Trim();

            // Extract price
            var priceText = document.DocumentNode.SelectSingleNode("//strong[@data-cy='adPageHeaderPrice']")?.InnerText.Trim();
            decimal price = 0;
            string? currency = null;
            if (!string.IsNullOrEmpty(priceText))
            {
                var match = Regex.Match(priceText, "(?<num>[0-9\\s\\.,]+)\\s*(?<cur>[^0-9\\s\\.,]+)?");
                if (match.Success)
                {
                    var num = match.Groups["num"].Value.Replace(" ", "").Replace('\u00A0', ' ').Replace(',', '.');
                    decimal.TryParse(num, NumberStyles.Number, CultureInfo.InvariantCulture, out price);
                    currency = match.Groups["cur"].Value?.Trim();
                }
            }

            // Extract description (fallback to address)
            var addressNode = document.DocumentNode.SelectSingleNode("//a[@data-sentry-element='StyledLink' and contains(@class, 'css-1eowip8')]");
            var description = addressNode != null ? $"🌍 {addressNode.InnerText.Trim()}" : null;

            // Extract ID
            var adId = document.DocumentNode.SelectSingleNode("//p[contains(@class, 'css-1fla28g') and contains(text(), 'ID')]")?.InnerText.Replace("ID: ", "").Trim();

            // Extract images
            var images = new List<Image>();
            var imageNodes = document.DocumentNode.SelectNodes("//img[contains(@class, 'css-1h8s4m6')]");
            if (imageNodes != null)
            {
                foreach (var imgNode in imageNodes)
                {
                    var src = imgNode.GetAttributeValue("src", string.Empty);
                    if (!string.IsNullOrEmpty(src))
                    {
                        var localPath = await _imageDownloader.DownloadImageAsync(src, "Images");
                        images.Add(new Image
                        {
                            OriginalUrl = src,
                            LocalFilePath = localPath,
                            FileName = Path.GetFileName(localPath),
                            IsPrimary = images.Count == 0
                        });
                    }
                }
            }

            return new Listing
            {
                Title = title,
                Price = price,
                Currency = currency,
                Description = description,
                PublicationDate = DateTime.Now,
                OriginalUrl = url,
                ExtractionDate = DateTime.UtcNow,
                ListingId = adId,
                Images = images
            };
        }

        public async Task<IEnumerable<string>> ExtractListingUrlsFromSearchAsync(string searchUrl, int maxResults = 10)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");

            var html = await client.GetStringAsync(searchUrl);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var results = new List<string>();
            var cardNodes = document.DocumentNode.SelectNodes("//a[@data-sentry-element='Link' and contains(@href, '/ro/oferta/')]");
            if (cardNodes != null)
            {
                foreach (var card in cardNodes)
                {
                    var href = card.GetAttributeValue("href", string.Empty);
                    var imageNode = card.SelectSingleNode(".//img[contains(@class, 'css-1h8s4m6')]");
                    var imageUrl = imageNode?.GetAttributeValue("src", string.Empty);

                    if (!string.IsNullOrEmpty(href))
                    {
                        if (href.StartsWith("/")) href = "https://www.storia.ro" + href;
                        if (!results.Contains(href))
                        {
                            results.Add(href);

                            // Optionally, download the primary image for each ad
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                await _imageDownloader.DownloadImageAsync(imageUrl, "Images"); // Changed to store in "Images" folder
                            }
                        }
                        if (results.Count >= maxResults) break;
                    }
                }
            }
            return results;
        }

        public async Task<string?> ExtractSearchKeywordFromUrlAsync(string searchUrl)
        {
            return await Task.Run(() => {
                var match = Regex.Match(searchUrl, "keywords\\]=([^&]+)");
                return match.Success ? Uri.UnescapeDataString(match.Groups[1].Value) : null;
            });
        }
    }
}