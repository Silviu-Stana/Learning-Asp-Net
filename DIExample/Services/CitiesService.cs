using ServiceContracts;

namespace Services
{
    public class CitiesService : ICitiesService
    {
        private List<string> _cities = new List<string>();

        private Guid _serviceInstanceId;
        public Guid InstanceId {
            get {  return _serviceInstanceId; }
        }

        public List<string> GetCities() => _cities;

        public CitiesService() {
            _serviceInstanceId = Guid.NewGuid();
            _cities = new List<string>
            {
                "London", "Paris", "New York", "Tokyo", "Rome"
            };
        }

        //public void Dispose()
        //{
        //    throw new NotImplementedException();
        //}
    }
}

