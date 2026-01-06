namespace Domain.Entities
{
    public class Image
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string? OriginalUrl { get; set; }
        public string? LocalFilePath { get; set; }
        public string? FileName { get; set; }
        public bool IsPrimary { get; set; }

        //Prevent infinite circular references
        [System.Text.Json.Serialization.JsonIgnore]
        public Listing? Listing { get; set; }
    }
}