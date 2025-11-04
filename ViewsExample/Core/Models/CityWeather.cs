using System.ComponentModel.DataAnnotations;

namespace ViewsExample.Models
{
    public class CityWeather
    {
        [Required]
        public string? CityUniqueCode;
        public string? CityName;
        public DateTime DateAndTime;
        public int TemperatureFahrenheit;
    }
}
