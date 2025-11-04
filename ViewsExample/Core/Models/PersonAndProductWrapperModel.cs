using System.ComponentModel.DataAnnotations;

namespace ViewsExample.Models
{
    public class PersonAndProductWrapperModel
    {
        public required Person PersonData { get; set; }
        public required Product ProductData { get; set; }
    }
}
