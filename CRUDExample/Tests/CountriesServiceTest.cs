using ServiceContracts;
using ServiceContracts.DTO;
using Entities;
using System;
using Services;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;

        public CountriesServiceTest()
        {
            _countriesService = new CountriesService(new PersonsDbContext(new DbContextOptionsBuilder<PersonsDbContext>().Options));
        }

        #region AddCountry
        //If Request null, throw exception
        [Fact]
        public async Task AddCountry_NullCountry()
        {
            CountryAddRequest? request = null;

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _countriesService.AddCountry(request);
            });
        }

        //Give null name, throw
        [Fact]
        public async Task AddCountry_NullCountryName()
        {
            CountryAddRequest? request = new() { CountryName = null };

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _countriesService.AddCountry(request);
            });
        }

        //If duplicate, throw
        [Fact]
        public async Task AddCountry_DuplicateName()
        {
            CountryAddRequest? request1 = new() { CountryName = "USA" };
            CountryAddRequest? request2 = new() { CountryName = "USA" };

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
            });
        }

        //If ok, insert in country list
        [Fact]
        public async Task AddCountry_ProperRequestWorks()
        {
            CountryAddRequest? request = new() { CountryName = "Japan" };

            CountryResponse response = await _countriesService.AddCountry(request);
            List<CountryResponse> countries = await _countriesService.GetAllCountries();

            Assert.True(response.CountryID != Guid.Empty);
            //objA.Equals(objB) compares references, not values, we need to override the Equals method
            Assert.Contains(response, countries);
        }
        #endregion

        #region GetAllCountries

        [Fact]
        public async Task GetAllCountries_ListIsEmptyByDefault()
        {
            List<CountryResponse> response = await _countriesService.GetAllCountries();
            Assert.Empty(response);
        }

        [Fact]
        public async Task GetAllCountries_ReturnMultipleCountries()
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
                countryResponses.Add(await _countriesService.AddCountry(countryRequest));
            }

            List<CountryResponse> actualCountries =  await _countriesService.GetAllCountries();

            Assert.Equal(countryRequestList.Count, actualCountries.Count);
            foreach (var request in countryRequestList)
            {
                Assert.Contains(actualCountries, c=>c.CountryName==request.CountryName);
            }
        }
        #endregion

        #region GetCountryById
        [Fact]
        public async Task GetCountryById_NullCountryId ()
        {
            Guid? countryId = null;
            CountryResponse? country = await _countriesService.GetCountryById(countryId);
            Assert.Null(country);
        }

        [Fact]
        public async Task GetCountryById_ValidRequest()
        {
            CountryAddRequest countryAddRequest = new() { CountryName="China"};
            CountryResponse addedCountry = await _countriesService.AddCountry(countryAddRequest);

            Guid? countryId = addedCountry.CountryID;

            CountryResponse? country = await _countriesService.GetCountryById(countryId);

            Assert.Equal(addedCountry, country);
        }
        #endregion
    }
}
