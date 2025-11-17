using ServiceContracts;
using ServiceContracts.DTO;
using Entities;
using System;
using Services;

namespace Tests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;

        public CountriesServiceTest()
        {
            _countriesService = new CountriesService(false);
        }

        #region AddCountry
        //If Request null, throw exception
        [Fact]
        public void AddCountry_NullCountry()
        {
            CountryAddRequest? request = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                _countriesService.AddCountry(request);
            });
        }

        //Give null name, throw
        [Fact]
        public void AddCountry_NullCountryName()
        {
            CountryAddRequest? request = new() { CountryName = null };

            Assert.Throws<ArgumentNullException>(() =>
            {
                _countriesService.AddCountry(request);
            });
        }

        //If duplicate, throw
        [Fact]
        public void AddCountry_DuplicateName()
        {
            CountryAddRequest? request1 = new() { CountryName = "USA" };
            CountryAddRequest? request2 = new() { CountryName = "USA" };

            Assert.Throws<ArgumentException>(() =>
            {
                _countriesService.AddCountry(request1);
                _countriesService.AddCountry(request2);
            });
        }

        //If ok, insert in country list
        [Fact]
        public void AddCountry_ProperRequestWorks()
        {
            CountryAddRequest? request = new() { CountryName = "Japan" };

            CountryResponse response = _countriesService.AddCountry(request);
            List<CountryResponse> countries = _countriesService.GetAllCountries();

            Assert.True(response.CountryID != Guid.Empty);
            //objA.Equals(objB) compares references, not values, we need to override the Equals method
            Assert.Contains(response, countries);
        }
        #endregion

        #region GetAllCountries

        [Fact]
        public void GetAllCountries_ListIsEmptyByDefault()
        {
            List<CountryResponse> response = _countriesService.GetAllCountries();
            Assert.Empty(response);
        }

        [Fact]
        public void GetAllCountries_ReturnMultipleCountries()
        {
            List<CountryAddRequest> countryRequestList = new()
            {
                new () {CountryName= "Romania"},
                new () {CountryName= "USA"},
                new () {CountryName= "Japan"},
            };

            List<CountryResponse> countryResponses = [];

            foreach (CountryAddRequest countryRequest in countryRequestList)
            {
                countryResponses.Add(_countriesService.AddCountry(countryRequest));
            }

            List<CountryResponse> actualCountries =  _countriesService.GetAllCountries();

            Assert.Equal(countryRequestList.Count, actualCountries.Count);
            foreach (var request in countryRequestList)
            {
                Assert.Contains(actualCountries, c=>c.CountryName==request.CountryName);
            }
        }
        #endregion

        #region GetCountryById
        [Fact]
        public void GetCountryById_NullCountryId ()
        {
            Guid? countryId = null;
            CountryResponse? country = _countriesService.GetCountryById(countryId);
            Assert.Null(country);
        }

        [Fact]
        public void GetCountryById_ValidRequest()
        {
            CountryAddRequest countryAddRequest = new() { CountryName="China"};
            CountryResponse addedCountry = _countriesService.AddCountry(countryAddRequest);

            Guid? countryId = addedCountry.CountryID;

            CountryResponse? country = _countriesService.GetCountryById(countryId);

            Assert.Equal(addedCountry, country);
        }
        #endregion
    }
}
