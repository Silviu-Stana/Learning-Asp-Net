using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Scrapers
{
    public class OlxScraper : IScraper
    {
        private readonly ImageDownloader _imageDownloader;

        public OlxScraper(ImageDownloader imageDownloader)
        {
            _imageDownloader = imageDownloader;
        }

        public async Task<Listing> ScrapeListingAsync(string url)
        {
            var web = new HtmlWeb();
            var document = await web.LoadFromWebAsync(url);

            // Extract title
            var title = document.DocumentNode.SelectSingleNode("//h4[@class='css-1l3a0i9']")?.InnerText.Trim();

            // Extract price and currency
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

            // Extract description
            var description = document.DocumentNode.SelectSingleNode("//div[@class='css-19duwlz']")?.InnerText.Trim();

            // Extract number of views (digits only) - try several selectors/fallbacks
            var views = 0;
            string? viewsText = null;
            var viewsNode = document.DocumentNode.SelectSingleNode("//span[@data-testid='page-view-counter']")
                            ?? document.DocumentNode.SelectSingleNode("//span[contains(@class,'css-16uueru')]")
                            ?? document.DocumentNode.SelectSingleNode("//span[contains(text(),'Vizualiz')]");
            if (viewsNode != null) viewsText = viewsNode.InnerText.Trim();
            if (string.IsNullOrEmpty(viewsText))
            {
                // try to find any node containing the word Vizualiz or Vizualizari
                var any = document.DocumentNode.SelectSingleNode("//*[contains(text(),'Vizualiz') or contains(text(),'Vizualizari')]");
                if (any != null) viewsText = any.InnerText;
            }
            if (!string.IsNullOrEmpty(viewsText))
            {
                var digits = Regex.Match(viewsText, "\\d+");
                if (digits.Success && int.TryParse(digits.Value, out var v)) views = v;
            }

            // Extract ad ID
            var adId = document.DocumentNode.SelectSingleNode("//span[@class='css-ooacec']")?.InnerText.Replace("ID: ", "").Trim();

            // Extract seller name
            var sellerName = document.DocumentNode.SelectSingleNode("//h4[@data-testid='user-profile-user-name']")?.InnerText.Trim();

            // Extract location - prefer the city / region elements, ignore seller-links and fallback to other selectors
            string? location = null;
            var loc1 = document.DocumentNode.SelectSingleNode("//p[@class='css-9pna1a']")?.InnerText.Trim();
            var loc2 = document.DocumentNode.SelectSingleNode("//p[@class='css-3cz5o2']")?.InnerText.Trim();
            // ignore obvious non-location texts
            bool IsBadLocation(string? s) => string.IsNullOrEmpty(s) || s.Contains("Mai multe anun", StringComparison.InvariantCultureIgnoreCase) || s.Contains("anun");
            if (!IsBadLocation(loc1) || !IsBadLocation(loc2))
            {
                location = string.Join(", ", new[] { loc1, loc2 }.Where(part => !string.IsNullOrEmpty(part)));
            }
            // fallback: try breadcrumb or meta tags
            if (string.IsNullOrEmpty(location))
            {
                var breadcrumb = document.DocumentNode.SelectSingleNode("//nav//a[last()]")?.InnerText.Trim();
                if (!string.IsNullOrEmpty(breadcrumb) && !IsBadLocation(breadcrumb)) location = breadcrumb;
            }
            if (string.IsNullOrEmpty(location))
            {
                var metaLoc = document.DocumentNode.SelectSingleNode("//meta[@property='og:locality']")?.GetAttributeValue("content", null)
                              ?? document.DocumentNode.SelectSingleNode("//meta[@name='location']")?.GetAttributeValue("content", null);
                if (!string.IsNullOrEmpty(metaLoc)) location = metaLoc;
            }
            if (string.IsNullOrEmpty(location)) location = loc2 ?? loc1;

            ExtractPublicationDate();

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


            void ExtractPublicationDate()
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
            }

            return new Listing
            {
                Title = title,
                Price = price,
                Currency = currency,
                Description = description,
                OriginalUrl = url,
                ExtractionDate = DateTime.UtcNow,
                SellerName = sellerName,
                Location = location,
                Views = views,
                ListingId = adId,
                Images = images
            };
        }
    }
}