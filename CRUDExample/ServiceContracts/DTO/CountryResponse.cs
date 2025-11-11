using System;
using Entities;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// DTO class as a return type for most of CountriesService methods
    /// </summary>
    public class CountryResponse
    {
        public Guid CountryID { get; set; }
        public string? CountryName { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if(obj.GetType() != typeof(CountryResponse)) return false;

            CountryResponse countryResponse = (CountryResponse)obj;

            return this.CountryID == countryResponse.CountryID && this.CountryName == countryResponse.CountryName;
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    public static class CountryExtensions
    {
        public static CountryResponse ToCountryResponse(this Country country)
        {
            return new CountryResponse { CountryID = country.CountryID, CountryName = country.CountryName };
        }
    }
}
