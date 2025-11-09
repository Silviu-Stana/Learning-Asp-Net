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
            _countriesService = new CountriesService();
        }

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
        public void AddCountry_Ok()
        {
            CountryAddRequest? request = new() { CountryName = "Japan" };

            CountryResponse response = _countriesService.AddCountry(request);

            Assert.True(response.CountryID != Guid.Empty);
        }
    }
}
