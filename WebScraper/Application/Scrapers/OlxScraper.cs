using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;

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
            // Fetch page HTML with a browser-like HttpClient to increase chance of server returning full content
            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ro-RO,ro;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Referrer = new System.Uri(url);

            var html = await client.GetStringAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(html);


            // Price & Currency
            var priceText = document.DocumentNode.SelectSingleNode("//h3[@class='css-1j840l6']")?.InnerText.Trim();
            decimal price = 0;
            string? currency = null;
            if (!string.IsNullOrEmpty(priceText))
            {
                // Examples: "1 800 lei", "€ 1.200", "1.200 €"
                var m = Regex.Match(priceText, @"(?<num>[0-9\s\.,]+)\s*(?<cur>[^0-9\s\.,]+)?");
                if (m.Success)
                {
                    var num = m.Groups["num"].Value;
                    var cur = m.Groups["cur"].Value?.Trim();
                    // normalize number (remove spaces)
                    num = num.Replace(" ", "").Replace("\u00A0", "");
                    // replace comma with dot if necessary
                    num = num.Replace(',', '.');
                    if (decimal.TryParse(num, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
                    {
                        price = parsed;
                    }
                    if (!string.IsNullOrEmpty(cur)) currency = cur;
                }
            }


            // --- 3. Robust Views Extraction ---
            var viewsNode = document.DocumentNode.SelectSingleNode("//span[@data-testid='page-view-counter']");
            int viewsCount = 0;
            if (viewsNode != null)
            {
                // Extracts digits from "Vizualizări: 10" -> "10"
                var match = Regex.Match(viewsNode.InnerText, @"\d+");
                if (match.Success) int.TryParse(match.Value, out viewsCount);
            }


            var title = document.DocumentNode.SelectSingleNode("//h4[@class='css-1l3a0i9']")?.InnerText.Trim();
            var description = document.DocumentNode.SelectSingleNode("//div[@class='css-19duwlz']")?.InnerText.Trim();
            var adId = document.DocumentNode.SelectSingleNode("//span[@class='css-ooacec']")?.InnerText.Replace("ID: ", "").Trim();
            var sellerName = document.DocumentNode.SelectSingleNode("//h4[@data-testid='user-profile-user-name']")?.InnerText.Trim();

            //Location
            var cityNode = document.DocumentNode.SelectSingleNode("//p[@class='css-9pna1a']");
            var countyNode = document.DocumentNode.SelectSingleNode("//p[@class='css-3cz5o2']");
            string city = cityNode?.InnerText.Trim() ?? "Unknown City";
            string county = countyNode?.InnerText.Trim() ?? "";
            string fullLocation = string.IsNullOrEmpty(county) ? city : $"{city}, {county}";

            var images = await ExtractImages(_imageDownloader);

            async Task<List<Image>> ExtractImages(ImageDownloader imageDownloader)
            {
                // Extract image URLs
                var imageNodes = document.DocumentNode.SelectNodes("//img[@data-testid='swiper-image']");
                var imageList = new List<Image>();

                if (imageNodes != null)
                {
                    foreach (var imgNode in imageNodes)
                    {
                        var imageUrl = imgNode.GetAttributeValue("src", string.Empty);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            // prefer the largest URL if srcset is available
                            var srcset = imgNode.GetAttributeValue("srcset", null);
                            if (!string.IsNullOrEmpty(srcset))
                            {
                                // take last entry in srcset
                                var parts = srcset.Split(',').Select(p => p.Trim()).ToArray();
                                var last = parts.LastOrDefault();
                                if (last != null)
                                {
                                    var urlPart = last.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(urlPart)) imageUrl = urlPart.Trim();
                                }
                            }

                            var localPath = await imageDownloader.DownloadImageAsync(imageUrl, "Images");
                            var safeFileName = Path.GetFileName(localPath);
                            imageList.Add(new Image
                            {
                                OriginalUrl = imageUrl,
                                LocalFilePath = localPath,
                                FileName = safeFileName,
                                IsPrimary = imageList.Count == 0
                            });
                        }
                    }
                }

                return imageList;
            }


            DateTime ExtractPublicationDate()
            {
                // Extract publication date
                var publicationDate = default(DateTime);
                var pubNode = document.DocumentNode.SelectSingleNode("//span[@data-testid='ad-posted-at' or @data-cy='ad-posted-at']")
                              ?? document.DocumentNode.SelectSingleNode("//span[contains(@class,'css-7b83xv')]");
                var pubText = pubNode?.InnerText?.Trim();
                if (!string.IsNullOrEmpty(pubText))
                {
                    pubText = pubText.Replace("\u00A0", " ").Trim();
                    // Examples: "Azi la 11:32", "Ieri la 09:10", "acum 2 zile", or full date
                    if (pubText.Contains("azi", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var m = Regex.Match(pubText, "(\\d{1,2}:\\d{2})");
                        if (m.Success && TimeSpan.TryParse(m.Groups[1].Value, out var ts))
                            publicationDate = DateTime.Today.Add(ts);
                        else
                            publicationDate = DateTime.Today;
                    }
                    else if (pubText.Contains("ieri", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var m = Regex.Match(pubText, "(\\d{1,2}:\\d{2})");
                        if (m.Success && TimeSpan.TryParse(m.Groups[1].Value, out var ts))
                            publicationDate = DateTime.Today.AddDays(-1).Add(ts);
                        else
                            publicationDate = DateTime.Today.AddDays(-1);
                    }
                    else if (pubText.StartsWith("acum", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var m = Regex.Match(pubText, "acum\\s+(\\d+)\\s*(\\w+)", RegexOptions.IgnoreCase);
                        if (m.Success && int.TryParse(m.Groups[1].Value, out var n))
                        {
                            var unit = m.Groups[2].Value.ToLower();
                            if (unit.StartsWith("min")) publicationDate = DateTime.Now.AddMinutes(-n);
                            else if (unit.StartsWith("or")) publicationDate = DateTime.Now.AddHours(-n);
                            else if (unit.StartsWith("zi")) publicationDate = DateTime.Now.AddDays(-n);
                            else publicationDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        // try parse with Romanian culture, allow formats with 'la'
                        var tryText = pubText.Replace(" la ", " ");
                        if (!DateTime.TryParse(tryText, new CultureInfo("ro-RO"), DateTimeStyles.AllowWhiteSpaces, out publicationDate))
                        {
                            // try common formats
                            var formats = new[] { "d MMM yyyy H:mm", "d MMM yyyy", "d MMM H:mm", "d MMM" };
                            foreach (var f in formats)
                            {
                                if (DateTime.TryParseExact(tryText, f, new CultureInfo("ro-RO"), DateTimeStyles.None, out publicationDate))
                                    break;
                            }
                        }
                    }
                }

                return publicationDate;
            }

            return new Listing
            {
                Title = title,
                Price = price, // from your regex logic
                Currency = currency,
                Description = document.DocumentNode.SelectSingleNode("//div[@class='css-19duwlz']")?.InnerText.Trim(),
                PublicationDate = ExtractPublicationDate(),
                OriginalUrl = url,
                ExtractionDate = DateTime.UtcNow,
                SellerName = sellerName,
                Location = fullLocation,
                Views = viewsCount,
                ListingId = adId,
                Images = images
            };
        }
    }
}