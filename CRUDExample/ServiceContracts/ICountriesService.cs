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
        CountryResponse AddCountry(CountryAddRequest? countryAddRequest);
    }
}
