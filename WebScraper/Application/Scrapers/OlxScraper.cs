using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using HtmlAgilityPack;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Application.Scrapers
{
    public class OlxScraper : IScraper
    {
        private readonly ImageDownloader _imageDownloader;
        private readonly IHttpClientFactory _httpClientFactory;

        public OlxScraper(ImageDownloader imageDownloader, IHttpClientFactory httpClientFactory)
        {
            _imageDownloader = imageDownloader;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Listing> ScrapeListingAsync(string url)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");

            var html = await client.GetStringAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(html);

            // Extract title
            var title = document.DocumentNode.SelectSingleNode("//h4[contains(@class,'css-1l3a0i9')]")?.InnerText.Trim()
                        ?? document.DocumentNode.SelectSingleNode("//h1")?.InnerText.Trim();

            var priceText = document.DocumentNode.SelectSingleNode("//h3[contains(@class,'css-1j840l6')]")?.InnerText.Trim();
            decimal price = 0;
            string? currency = null;
            if (!string.IsNullOrEmpty(priceText))
            {
                var m = Regex.Match(priceText, "(?<num>[0-9\\s\\.,]+)\\s*(?<cur>[^0-9\\s\\.,]+)?");
                if (m.Success)
                {
                    var num = m.Groups["num"].Value.Replace(" ", "").Replace('\u00A0', ' ').Replace(',', '.');
                    if (decimal.TryParse(num, NumberStyles.Number, CultureInfo.InvariantCulture, out var p)) price = p;
                    currency = m.Groups["cur"].Value?.Trim();
                }
            }

            var description = document.DocumentNode.SelectSingleNode("//div[contains(@class,'css-19duwlz')]")?.InnerText.Trim();

            var viewsNode = document.DocumentNode.SelectSingleNode("//span[@data-testid='ad-views-counter']")
                          ?? document.DocumentNode.SelectSingleNode("//div[contains(@class, 'css-') and contains(., 'Vizualizări')]");


            var adId = document.DocumentNode.SelectSingleNode("//span[contains(@class,'css-ooacec')]")?.InnerText.Replace("ID: ", "").Trim();

            string views = "-";  // ex: "Vizualizări: 2960"
            if (!string.IsNullOrEmpty(adId))
            {
                views = await GetLiveViewsAsync(adId, url);
            }

            var sellerName = document.DocumentNode.SelectSingleNode("//h4[@data-testid='user-profile-user-name']")?.InnerText.Trim();

            string city = "-";

            // Attempt to get city from og:title metadata (e.g., "Incalzire cusca caine Cluj-Napoca • OLX.ro")
            var metaTitle = document.DocumentNode.SelectSingleNode("//meta[@property='og:title']")?
                            .GetAttributeValue("content", "");

            if (!string.IsNullOrEmpty(metaTitle))
            {
                // Regex to extract the city between the last space/title and the bullet point '•'
                var match = Regex.Match(metaTitle, @"\s+([A-Z][a-z]+(-[A-Z][a-z]+)?)\s+•");
                if (match.Success)
                {
                    city = match.Groups[1].Value;
                }
            }


            // images
            var images = new List<Image>();
            try
            {
                var imageNodes = document.DocumentNode.SelectNodes("//img[@data-testid='swiper-image']")
                                 ?? document.DocumentNode.SelectNodes("//img[contains(@class,'css-8wsg1m')]")
                                 ?? document.DocumentNode.SelectNodes("//div[contains(@class,'css-gl6djm')]//img");

                if (imageNodes != null)
                {
                    foreach (var imgNode in imageNodes)
                    {
                        var src = imgNode.GetAttributeValue("src", string.Empty) ?? imgNode.GetAttributeValue("data-src", string.Empty);
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
            }
            catch { }

            return new Listing
            {
                Title = title,
                Price = price,
                Currency = currency,
                Description = description,
                PublicationDate = DateTime.Now,
                OriginalUrl = url,
                ExtractionDate = DateTime.UtcNow,
                SellerName = sellerName,
                Location = city,
                Views = views,
                ListingId = adId,
                Images = images
            };
        }

        public async Task<IEnumerable<string>> ExtractListingUrlsFromSearchAsync(string searchUrl, int maxResults = 10)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");

            var html = await client.GetStringAsync(searchUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var results = new List<string>();

            var cardNodes = doc.DocumentNode.SelectNodes("//div[@data-cy='l-card']")
                            ?? doc.DocumentNode.SelectNodes("//div[contains(@class,'css-1sw7q4x')]");

            if (cardNodes != null)
            {
                foreach (var card in cardNodes)
                {
                    try
                    {
                        var a = card.SelectSingleNode(".//a[contains(@class,'css-1tqlkj0') or contains(@href,'/d/oferta')]");
                        if (a != null)
                        {
                            var href = a.GetAttributeValue("href", string.Empty);
                            if (!string.IsNullOrEmpty(href))
                            {
                                if (href.StartsWith("/")) href = new Uri(new Uri("https://www.olx.ro"), href).ToString();
                                if (!results.Contains(href)) results.Add(href);
                                if (results.Count >= maxResults) break;
                            }
                        }
                    }
                    catch { }
                }
            }

            return results.Take(maxResults).ToList();
        }

        public async Task<string?> ExtractSearchKeywordFromUrlAsync(string searchUrl)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var uri = new Uri(searchUrl);
                    var path = uri.AbsolutePath; // preserves the original path
                    var idx = path.IndexOf("/q-", StringComparison.OrdinalIgnoreCase);
                    if (idx < 0) return null;
                    var start = idx + 3; // position of 'q-'
                    var rest = path.Substring(start);
                    var endIdx = rest.IndexOf('/');
                    var keyword = endIdx >= 0 ? rest.Substring(0, endIdx) : rest;
                    return Uri.UnescapeDataString(keyword);
                }
                catch
                {
                    var m = Regex.Match(searchUrl, "/q-([^/]+)");
                    if (m.Success) return Uri.UnescapeDataString(m.Groups[1].Value);
                    return null;
                }
            });
        }

        public async Task<string> GetLiveViewsAsync(string adId, string adUrl)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", adUrl); // Critic pentru a nu primi 403

            string statsUrl = $"https://www.olx.ro/api/v1/targeting/ad-statistics/ad/{adId}/";

            try
            {
                var response = await client.GetAsync(statsUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("views", out var views))
                    {
                        return views.ToString();
                    }
                }
            }
            catch { }
            return "-";
        }
    }


}
