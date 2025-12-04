using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly PersonsDbContext _db;
        public CountriesService(PersonsDbContext personsDbContext)
        {
            _db = personsDbContext;
        }
        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            if (countryAddRequest == null) throw new ArgumentNullException(nameof(countryAddRequest));
            if (countryAddRequest.CountryName == null) throw new ArgumentNullException(countryAddRequest.CountryName);
            if (await _db.Countries.CountAsync(c => c.CountryName == countryAddRequest.CountryName)>0) throw new ArgumentException("Country name already exists");

            Country country = countryAddRequest.ToCountry();

            country.CountryID = Guid.NewGuid();

            _db.Countries.Add(country);
            await _db.SaveChangesAsync();

            return country.ToCountryResponse();
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            return await _db.Countries.Select(country => country.ToCountryResponse()).ToListAsync();
        }

        public async Task<CountryResponse?> GetCountryById(Guid? countryId)
        {
            if (countryId == null) return null;

            Country? country = await _db.Countries.FirstOrDefaultAsync(c => c.CountryID == countryId);

            return country?.ToCountryResponse();
        }

        public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
        {
            MemoryStream stream = new();

            await formFile.CopyToAsync(stream);

            int countriesInserted = 0;

            using (ExcelPackage package = new(stream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Countries"];

                int rowCount = worksheet.Dimension.Rows;
                

                for (int row = 2; row <= rowCount; row++)
                {
                    string? cellValue = Convert.ToString(worksheet.Cells[row, 1].Value);

                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        string countryName = cellValue;

                        if(!await _db.Countries.AnyAsync(c => c.CountryName == countryName))
                        {
                            Country country = new() { CountryName = countryName};
                            _db.Countries.Add(country);
                            await _db.SaveChangesAsync();
                            countriesInserted++;
                        }
                    }
                }
                return countriesInserted;
            }
        }



    }
}
