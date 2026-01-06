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
    public class StoriaScraperSlow : IScraper
    {
        private readonly ImageDownloader _imageDownloader;

        public StoriaScraperSlow(ImageDownloader imageDownloader)
        {
            _imageDownloader = imageDownloader;
        }

        private IWebDriver CreateDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            return new ChromeDriver(options);
        }

        public async Task<Listing> ScrapeListingAsync(string url)
        {
            using var driver = CreateDriver();
            driver.Navigate().GoToUrl(url);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // Extract title
            var title = driver.FindElement(By.XPath("//h1[@data-cy='adPageAdTitle']"))?.Text.Trim();

            // Extract price
            var priceText = driver.FindElement(By.XPath("//strong[@data-cy='adPageHeaderPrice']"))?.Text.Trim();
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

            // Extract description
            var descriptionNode = driver.FindElement(By.XPath("//span[contains(@class, 'css-ziqzm') and contains(@class, 'e1op7yyl4')]")).GetAttribute("innerHTML");
            var description = descriptionNode?.Replace("<br>", "\n").Trim();

            // Extract ID
            var adId = driver.FindElement(By.XPath("//p[contains(@class, 'css-1fla28g') and contains(text(), 'ID')]")).Text.Replace("ID: ", "").Trim();

            // Extract images
            var images = new List<Image>();
            var imageNodes = driver.FindElements(By.XPath("//img[contains(@class, 'css-1h8s4m6')]"));
            foreach (var imgNode in imageNodes)
            {
                var src = imgNode.GetAttribute("src");
                if (!string.IsNullOrEmpty(src))
                {
                    var localPath = await _imageDownloader.DownloadImageAsync(src, "StoriaImages");
                    images.Add(new Image
                    {
                        OriginalUrl = src,
                        LocalFilePath = localPath,
                        FileName = Path.GetFileName(localPath),
                        IsPrimary = images.Count == 0
                    });
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
            using var driver = CreateDriver();
            driver.Navigate().GoToUrl(searchUrl);

            var results = new List<string>();
            var cardNodes = driver.FindElements(By.XPath("//a[@data-sentry-element='Link' and contains(@href, '/ro/oferta/')]"));
            foreach (var card in cardNodes)
            {
                var href = card.GetAttribute("href");
                var imageNode = card.FindElement(By.XPath(".//img[@data-cy='listing-item-image-source']"));
                var imageUrl = imageNode?.GetAttribute("src");

                if (!string.IsNullOrEmpty(href))
                {
                    if (href.StartsWith("/")) href = "https://www.storia.ro" + href;
                    if (!results.Contains(href))
                    {
                        results.Add(href);

                        // Optionally, download the primary image for each ad
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            await _imageDownloader.DownloadImageAsync(imageUrl, "StoriaSearchImages");
                        }
                    }
                    if (results.Count >= maxResults) break;
                }
            }
            return results;
        }

        public async Task<string?> ExtractSearchKeywordFromUrlAsync(string searchUrl)
        {
            return await Task.Run(() => {
                // Storia search URLs usually look like /ro/rezultate/vanzare/apartament/bucuresti?search[keywords]=casa
                var m = Regex.Match(searchUrl, @"keywords\]=([^&]+)");
                return m.Success ? Uri.UnescapeDataString(m.Groups[1].Value) : null;
            });
        }
    }
}