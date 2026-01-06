using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Scrapers
{
    public class OlxScraper : IScraper
    {
        private readonly ImageDownloader _imageDownloader;
        private readonly IWebDriver _driver;

        public OlxScraper(ImageDownloader imageDownloader)
        {
            _imageDownloader = imageDownloader;

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless=new");
            chromeOptions.AddArgument("--disable-gpu");
            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("--disable-dev-shm-usage");
            chromeOptions.AddArgument("--window-size=1920,1080");

            _driver = new ChromeDriver(chromeOptions);
        }

        public async Task<Listing> ScrapeListingAsync(string url)
        {
            _driver.Navigate().GoToUrl(url);

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // wait for title
            wait.Until(d => d.FindElement(By.CssSelector("h4.css-1l3a0i9")));

            string GetText(By selector)
            {
                try { return _driver.FindElement(selector).Text.Trim(); } catch { return string.Empty; }
            }

            var title = GetText(By.CssSelector("h4.css-1l3a0i9"));
            var priceText = GetText(By.CssSelector("h3.css-1j840l6"));
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

            var description = string.Empty;
            try { description = _driver.FindElement(By.CssSelector("div.css-19duwlz")).Text.Trim(); } catch { }

            var views = 0;
            try
            {
                var vText = _driver.FindElement(By.CssSelector("span[data-testid='page-view-counter']")).Text;
                var digitsOnly = Regex.Replace(vText, "\\D+", "");
                if (!string.IsNullOrEmpty(digitsOnly) && int.TryParse(digitsOnly, out var vv)) views = vv;
            }
            catch { }

            var adId = string.Empty;
            try { adId = _driver.FindElement(By.CssSelector("span.css-ooacec")).Text.Replace("ID: ", "").Trim(); } catch { }

            var sellerName = string.Empty;
            try { sellerName = _driver.FindElement(By.CssSelector("h4[data-testid='user-profile-user-name']")).Text.Trim(); } catch { }

            string city = string.Empty, county = string.Empty;
            try { city = _driver.FindElement(By.CssSelector("p.css-9pna1a")).Text.Trim(); } catch { }
            try { county = _driver.FindElement(By.CssSelector("p.css-3cz5o2")).Text.Trim(); } catch { }
            var fullLocation = string.IsNullOrEmpty(county) ? city : $"{city}, {county}";

            // images
            var images = new List<Image>();
            try
            {
                var imageElements = _driver.FindElements(By.CssSelector("img[data-testid='swiper-image']"));
                foreach (var imgEl in imageElements)
                {
                    var src = imgEl.GetAttribute("src");
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
                Location = fullLocation,
                Views = views,
                ListingId = adId,
                Images = images
            };

            return listing;
        }
    }
}
