using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using HtmlAgilityPack;
using System.Net.Http;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
// Selenium removed: using HtmlAgilityPack + HttpClient instead

namespace Application.Scrapers
{
    public class OlxScraper : IScraper
    {
        private readonly ImageDownloader _imageDownloader;
        private readonly IHttpClientFactory _httpFactory;

        public OlxScraper(ImageDownloader imageDownloader, IHttpClientFactory httpFactory)
        {
            _imageDownloader = imageDownloader;
            _httpFactory = httpFactory;
        }

        public async Task<Listing> ScrapeListingAsync(string url)
        {
            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ro-RO,ro;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Referrer = new Uri(url);

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
                var m = Regex.Match(priceText, @"(?<num>[0-9\s\.,]+)\s*(?<cur>[^0-9\s\.,]+)?");
                if (m.Success)
                {
                    var num = m.Groups["num"].Value.Replace(" ", "").Replace('\u00A0', ' ');
                    num = num.Replace(',', '.');
                    if (decimal.TryParse(num, NumberStyles.Number, CultureInfo.InvariantCulture, out var p)) price = p;
                    currency = m.Groups["cur"].Value?.Trim();
                }
            }

            var description = document.DocumentNode.SelectSingleNode("//div[contains(@class,'css-19duwlz')]")?.InnerText.Trim();

            // Extract views
            var views = 0;
            string? viewsText = document.DocumentNode.SelectSingleNode("//span[@data-testid='page-view-counter']")?.InnerText
                                ?? document.DocumentNode.SelectSingleNode("//span[contains(@class,'css-16uueru')]")?.InnerText
                                ?? document.DocumentNode.SelectSingleNode("//*[contains(text(),'Vizualiz')]")?.InnerText;
            if (!string.IsNullOrEmpty(viewsText))
            {
                var digitsOnly = Regex.Replace(viewsText, "\\D+", "");
                if (!string.IsNullOrEmpty(digitsOnly) && int.TryParse(digitsOnly, out var v)) views = v;
            }

            var adId = document.DocumentNode.SelectSingleNode("//span[contains(@class,'css-ooacec')]")?.InnerText.Replace("ID: ", "").Trim();
            var sellerName = document.DocumentNode.SelectSingleNode("//h4[@data-testid='user-profile-user-name']")?.InnerText.Trim();

            string city = document.DocumentNode.SelectSingleNode("//p[contains(@class,'css-9pna1a')]")?.InnerText.Trim() ?? string.Empty;

            // images
            var images = new List<Image>();
            try
            {
                // Try a few selectors for images
                var imageNodes = document.DocumentNode.SelectNodes("//img[@data-testid='swiper-image']")
                                 ?? document.DocumentNode.SelectNodes("//img[contains(@class,'css-8wsg1m')]")
                                 ?? document.DocumentNode.SelectNodes("//div[contains(@class,'css-gl6djm')]//img");

                if (imageNodes != null)
                {
                    foreach (var imgNode in imageNodes)
                    {
                        var src = imgNode.GetAttributeValue("src", null) ?? imgNode.GetAttributeValue("data-src", string.Empty);
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

            var listing = new Listing
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

            return listing;
        }

        public async Task<IEnumerable<string>> ExtractListingUrlsFromSearchAsync(string searchUrl)
        {
            // Use HttpClient + HtmlAgilityPack to load search page and extract first 10 listing hrefs
            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ro-RO,ro;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Referrer = new Uri(searchUrl);

            var html = await client.GetStringAsync(searchUrl);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var results = new List<string>();

            // Prefer data-cy cards
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
                                if (results.Count >= 10) break;
                            }
                        }
                    }
                    catch { }
                }
            }

            return results.Take(10).ToList();
        }

        public async Task<string?> ExtractSearchKeywordFromUrlAsync(string searchUrl)
        {
            // If the URL contains /q- then extract from q- to next slash
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
                // fallback: try regex
                var m = Regex.Match(searchUrl, "/q-([^/]+)");
                if (m.Success) return Uri.UnescapeDataString(m.Groups[1].Value);
                return null;
            }
        }
    }
}
