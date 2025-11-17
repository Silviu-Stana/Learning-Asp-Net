using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly List<Country> _countries = [];
        public CountriesService(bool initialize = true)
        {
            _countries = new List<Country>();
            if (initialize)
            {
                _countries.AddRange(new List<Country>()
                {
                    new Country() { CountryID = Guid.Parse("FAE1E3D1-3FEA-47C8-A3E0-095E827E1170"), CountryName = "USA" },
                    new Country() { CountryID = Guid.Parse("15A1F30A-BC16-4CEF-9AFC-137E41EE7812"), CountryName = "UK" },
                    new Country() { CountryID = Guid.Parse("B06A3323-C6EF-4073-89BC-43942764EBC5"), CountryName = "Germany" },
                    new Country() { CountryID = Guid.Parse("FC3F2A77-7CA3-4C2E-A8BC-10C1E52F9210"), CountryName = "Denmark" }
                });
            }
        }
        public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
        {
            if (countryAddRequest == null) throw new ArgumentNullException(nameof(countryAddRequest));
            if (countryAddRequest.CountryName == null) throw new ArgumentNullException(countryAddRequest.CountryName);
            if (_countries.Any(c => c.CountryName == countryAddRequest.CountryName)) throw new ArgumentException("Country name already exists");

            Country country = countryAddRequest.ToCountry();

            country.CountryID = Guid.NewGuid();

            _countries.Add(country);

            return country.ToCountryResponse();
        }

        public List<CountryResponse> GetAllCountries()
        {
            return _countries.Select(country => country.ToCountryResponse()).ToList();
        }

        public CountryResponse? GetCountryById(Guid? countryId)
        {
            if (countryId == null) return null;

            Country? country = _countries.FirstOrDefault(c => c.CountryID == countryId);

            return country?.ToCountryResponse();
        }
    }
}
