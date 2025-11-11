using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly List<Country> _countries;

        public CountriesService()
        {
            _countries = new List<Country>();
        }

        public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
        {
            if(countryAddRequest == null) throw new ArgumentNullException(nameof(countryAddRequest));
            if(countryAddRequest.CountryName == null) throw new ArgumentNullException(countryAddRequest.CountryName);
            if (_countries.Where(c => c.CountryName == countryAddRequest.CountryName).Any()) throw new ArgumentException("Country name already exists");

            Country country = countryAddRequest.ToCountry();

            country.CountryID = Guid.NewGuid();

            _countries.Add(country);

            return country.ToCountryResponse();
        }

        public List<CountryResponse> GetAllCountries()
        {
            return _countries.Select(country=>country.ToCountryResponse()).ToList();
        }

        public CountryResponse? GetCountryById(Guid? countryId)
        {
            if (countryId == null) return null;

            Country? country = _countries.FirstOrDefault(c => c.CountryID == countryId);

            return country?.ToCountryResponse();
        }
    }
}
