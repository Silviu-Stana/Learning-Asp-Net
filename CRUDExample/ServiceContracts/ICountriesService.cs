using ServiceContracts.DTO;

namespace ServiceContracts
{
    /// <summary>
    /// Provides methods for accessing and managing country-related data.
    /// </summary>
    public interface ICountriesService
    {
        /// <summary>
        /// Adds a country object to the list of countries.
        /// </summary>
        /// <param name="countryAddRequest">The request containing the details of the country to be added.</param>
        /// <returns>A <see cref="CountryResponse"/></returns>
        Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);


        /// <summary>
        /// Retrieves a list of all countries.
        /// </summary>
        /// <returns>A list of <see cref="CountryResponse"/> objects representing all available countries. The list will be empty
        /// if no countries are available.</returns>
        Task<List<CountryResponse>> GetAllCountries();


        /// <summary>
        /// Returns a country object based on id.
        /// </summary>
        /// <param name="countryId">CountryID to search</param>
        /// <returns>Match as a CountryResponse object.</returns>
        Task<CountryResponse?> GetCountryById(Guid? countryId);
    }
}
