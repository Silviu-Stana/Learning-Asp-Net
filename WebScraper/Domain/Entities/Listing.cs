namespace Domain.Entities
{
    public class Listing
    {
        //Autoincrements
        public int Id { get; set; }
        public string? Title { get; set; }
        public decimal Price { get; set; }
        public string? Currency { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public DateTime PublicationDate { get; set; }
        public string? SellerName { get; set; }
        public string? SellerPhone { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public DateTime ExtractionDate { get; set; }

        public int Views { get; set; }

        //Helps prevent duplicates when scraping data.
        public string? ListingId { get; set; }

        public ICollection<Image>? Images { get; set; }
    }
}