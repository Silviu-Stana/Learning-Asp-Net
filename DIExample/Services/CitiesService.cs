using ServiceContracts;

namespace Services
{
    public class CitiesService : ICitiesService
    {
        private List<string> _cities = new List<string>();

        public CitiesService() {
            _cities = new List<string>
            {
                "London", "Paris", "New York", "Tokyo", "Rome"
            };
        }

        public List<string> GetCities() => _cities;
    }
}
