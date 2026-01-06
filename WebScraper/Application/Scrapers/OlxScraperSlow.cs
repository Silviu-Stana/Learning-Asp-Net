using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Scrapers
{
    public class OlxScraperSlow : IScraper
    {
        private readonly ImageDownloader _imageDownloader;

        public OlxScraperSlow(ImageDownloader imageDownloader)
        {
            _imageDownloader = imageDownloader;
        }

        private IWebDriver CreateDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new"); // Runs without a visible window
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--window-size=1920,1080");
            // Important: Set a real user agent to prevent blocks
            options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            return new ChromeDriver(options);
        }

        public async Task<Listing> ScrapeListingAsync(string url)
        {
            using var driver = CreateDriver();
            driver.Navigate().GoToUrl(url);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            try
            {
                // 1. Scroll to the bottom to trigger lazy-loaded elements
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

                // 2. Wait specifically for the view counter to contain text
                // We wait for the span to exist AND contain the word "Vizualizări"
                wait.Until(d => {
                    var element = d.FindElement(By.XPath("//span[@data-testid='page-view-counter']"));
                    return element.Displayed && !string.IsNullOrWhiteSpace(element.Text) && element.Text.Contains(":");
                });
            }
            catch (WebDriverTimeoutException)
            {
                // If it fails to load, we proceed anyway with whatever data we have
            }

            var html = driver.PageSource;
            var document = new HtmlDocument();
            document.LoadHtml(html);

            // --- Extraction using HtmlAgilityPack on the rendered PageSource ---

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
                    var num = m.Groups["num"].Value.Replace(" ", "").Replace('\u00A0', ' ').Replace(',', '.');
                    if (decimal.TryParse(num, NumberStyles.Number, CultureInfo.InvariantCulture, out var p)) price = p;
                    currency = m.Groups["cur"].Value?.Trim();
                }
            }

            var description = document.DocumentNode.SelectSingleNode("//div[contains(@class,'css-19duwlz')]")?.InnerText.Trim();

            // Extract Views
            var viewsNode = document.DocumentNode.SelectSingleNode("//span[@data-testid='page-view-counter']");
            string views = viewsNode != null ? Regex.Match(viewsNode.InnerText, @"\d+").Value : "0";

            var adId = document.DocumentNode.SelectSingleNode("//span[contains(@class,'css-ooacec')]")?.InnerText.Replace("ID: ", "").Trim();
            var sellerName = document.DocumentNode.SelectSingleNode("//h4[@data-testid='user-profile-user-name']")?.InnerText.Trim();
            string city = document.DocumentNode.SelectSingleNode("//p[contains(@class,'css-9pna1a')]")?.InnerText.Trim() ?? string.Empty;

            // Images
            var images = new List<Image>();
            var imageNodes = document.DocumentNode.SelectNodes("//img[@data-testid='swiper-image']")
                             ?? document.DocumentNode.SelectNodes("//img[contains(@class,'css-8wsg1m')]");

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
                            FileName = System.IO.Path.GetFileName(localPath),
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
                SellerName = sellerName,
                Location = city,
                Views = views,
                ListingId = adId,
                Images = images
            };
        }

        public async Task<IEnumerable<string>> ExtractListingUrlsFromSearchAsync(string searchUrl, int maxResults = 10)
        {
            using var driver = CreateDriver();
            driver.Navigate().GoToUrl(searchUrl);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            try { wait.Until(d => d.FindElements(By.XPath("//div[@data-cy='l-card']")).Count > 0); } catch { }

            var doc = new HtmlDocument();
            doc.LoadHtml(driver.PageSource);

            var results = new List<string>();
            var cardNodes = doc.DocumentNode.SelectNodes("//div[@data-cy='l-card']")
                            ?? doc.DocumentNode.SelectNodes("//div[contains(@class,'css-1sw7q4x')]");

            if (cardNodes != null)
            {
                foreach (var card in cardNodes)
                {
                    var a = card.SelectSingleNode(".//a");
                    if (a != null)
                    {
                        var href = a.GetAttributeValue("href", string.Empty);
                        if (!string.IsNullOrEmpty(href))
                        {
                            if (href.StartsWith("/")) href = "https://www.olx.ro" + href;
                            if (!results.Contains(href)) results.Add(href);
                            if (results.Count >= maxResults) break;
                        }
                    }
                }
            }
            return results;
        }

        public async Task<string?> ExtractSearchKeywordFromUrlAsync(string searchUrl)
        {
            return await Task.Run(() => {
                var m = Regex.Match(searchUrl, "/q-([^/]+)");
                return m.Success ? Uri.UnescapeDataString(m.Groups[1].Value) : null;
            });
        }
    }
}