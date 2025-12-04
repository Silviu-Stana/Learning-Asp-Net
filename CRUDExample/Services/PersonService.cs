using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Services
{
    public class PersonService : IPersonService
    {
        private readonly PersonsDbContext _db;
        private readonly ICountriesService _countriesService;

        public PersonService(PersonsDbContext personsDbContext, ICountriesService countriesService)
        {
            _db = personsDbContext;
            _countriesService = countriesService;
        }

        

        public async Task<PersonResponse> AddPerson(PersonAddRequest? request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            
            ValidationHelper.CheckForValidationErrors(request);

            Person person = request.ToPerson();

            person.Id = Guid.NewGuid();


            _db.Persons.Add(person);
            await _db.SaveChangesAsync();
            //_db.sp_InsertPerson(person);

            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            var persons = await _db.Persons.Include("Country").ToListAsync();

            return persons.Select(person => person.ToPersonResponse()).ToList();
        }

        public async Task<PersonResponse?> GetPersonById(Guid? personId)
        {
            if (personId == null) return null;

            Person? person = await _db.Persons.Include("Country").FirstOrDefaultAsync(x => x.Id == personId);
            
            if (person == null) return null;

            return person.ToPersonResponse();
        }

        #region SortingFunctions
        private List<PersonResponse> FilterByName(List<PersonResponse> allPeople, string searchString)
        {
            return allPeople.Where(p => !string.IsNullOrEmpty(p.Name) ? p.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
        }

        private List<PersonResponse> FilterByDateOfBirth(List<PersonResponse> allPeople, string searchString)
        {
            return allPeople.Where(p => (p.DateOfBirth != null) ? p.DateOfBirth.Value.ToString("dd MMMM yyyy").Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
        }

        private List<PersonResponse> FilterByGender(List<PersonResponse> allPeople, string searchString)
        {
            return allPeople.Where(p => !string.IsNullOrEmpty(p.Gender) ? p.Gender.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
        }

        private List<PersonResponse> FilterByCountry(List<PersonResponse> allPeople, string searchString)
        {
            return allPeople.Where(p => !string.IsNullOrEmpty(p.CountryName) && p.CountryName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private List<PersonResponse> FilterByAddress(List<PersonResponse> allPeople, string searchString)
        {
            return allPeople.Where(p => !string.IsNullOrEmpty(p.Address) ? p.Address.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
        }

        private List<PersonResponse> FilterByEmail(List<PersonResponse> allPeople, string searchString)
        {
            return allPeople.Where(p => !string.IsNullOrEmpty(p.Email) ? p.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
        }




        #endregion

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            List<PersonResponse> all = await GetAllPersons();
            List<PersonResponse> matchingPersons = all;

            if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString)) return matchingPersons;

            switch (searchBy)
            {
                case nameof(PersonResponse.Name):
                    matchingPersons = FilterByName(all, searchString);
                    break;
                case nameof(PersonResponse.DateOfBirth):
                    matchingPersons = FilterByDateOfBirth(all, searchString);
                    break;
                case nameof(PersonResponse.Gender):
                    matchingPersons = FilterByGender(all, searchString);
                    break;
                case nameof(PersonResponse.CountryName):
                    matchingPersons = FilterByCountry(all, searchString);
                    break;
                case nameof(PersonResponse.Address):
                    matchingPersons = FilterByAddress(all, searchString);
                    break;
                case nameof(PersonResponse.Email):
                    matchingPersons = FilterByEmail(all, searchString);
                    break;
                default:
                    matchingPersons = all;
                    break;
            }

            return matchingPersons;
        }

        public List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
        {
            if(string.IsNullOrEmpty(sortBy)) return allPersons;

            //Use reflection, otherwise there's way too many properties for a switch statement.
            var prop = typeof(PersonResponse).GetProperty(sortBy);
            if(prop == null) return allPersons;

            bool isString = prop.PropertyType == typeof(string);

            if(sortOrder == SortOrderOptions.ASC)
            {
                return isString ? allPersons.OrderBy(p => (string?)prop.GetValue(p), StringComparer.OrdinalIgnoreCase).ToList()
                                        : allPersons.OrderBy(p=>prop.GetValue(p)).ToList();
            }
            else
            {
                return isString ? allPersons.OrderByDescending(p => (string?)prop.GetValue(p), StringComparer.OrdinalIgnoreCase).ToList()
                                        : allPersons.OrderByDescending(p => prop.GetValue(p)).ToList();
            }
        }

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? request)
        {
            if (request == null) throw new ArgumentNullException(nameof(Person));

            ValidationHelper.CheckForValidationErrors(request);

            Person? match = await _db.Persons.FirstOrDefaultAsync(p => p.Id == request.PersonID);
            if (match == null) throw new ArgumentException("Given Person ID does not exists");

            //Update
            match.Name = request.Name;
            match.Email = request.Email;
            match.Address = request.Address;
            match.Gender = request.Gender.ToString();
            match.DateOfBirth = request.DateOfBirth;
            match.CountryID = request.CountryID;
            match.ReceiveNewsLetters = request.ReceiveNewsLetters;
            
            await _db.SaveChangesAsync();

            return match.ToPersonResponse();
        }

        public async Task<bool> DeletePerson(Guid? personId)
        {
            if (personId == null) throw new ArgumentNullException(nameof(personId));

            Person? person = await _db.Persons.FirstOrDefaultAsync(p => p.Id == personId);

            if(person == null) return false;

            _db.Persons.Remove(_db.Persons.First(p => p.Id == personId));
            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<MemoryStream> GetPersonsCSV()
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);

            CsvWriter csvWriter = new CsvWriter(writer, csvConfiguration, leaveOpen: true);

            //Writing the Header (first row of the file)
            //This way we control what fields we put in, for example, we excluded ID and Gender.
            csvWriter.WriteField(nameof(PersonResponse.Name));
            csvWriter.WriteField(nameof(PersonResponse.Email));
            csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
            csvWriter.WriteField(nameof(PersonResponse.Age));
            csvWriter.WriteField(nameof(PersonResponse.CountryName));
            csvWriter.WriteField(nameof(PersonResponse.Address));
            csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetters));
            csvWriter.NextRecord(); //move to next line

            List<PersonResponse> persons = await _db.Persons.Include("Country").Select(p=>p.ToPersonResponse()).ToListAsync();

            foreach (PersonResponse person in persons)
            {
                csvWriter.WriteField(person.Name);
                csvWriter.WriteField(person.Email);
                if (person.DateOfBirth.HasValue) csvWriter.WriteField(person.DateOfBirth.Value.ToString("yyyy-MM-dd"));
                else csvWriter.WriteField("");
                csvWriter.WriteField(person.Age);
                csvWriter.WriteField(person.CountryName);
                csvWriter.WriteField(person.Address);
                csvWriter.WriteField(person.ReceiveNewsLetters);
                csvWriter.NextRecord();//go to next line
                csvWriter.Flush();//add it to the memory stream
            }


            //After above, cursor will be at the end. If we don't reset it, clicking export a 2nd time would print empty.
            stream.Position = 0;
            return stream;
        }

        public async Task<MemoryStream> GetPersonsExcel()
        {
            MemoryStream stream = new();
            
            using(ExcelPackage excelPackage = new(stream))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");

                worksheet.Cells["A1"].Value = "Name";
                worksheet.Cells["B1"].Value = "Email";
                worksheet.Cells["C1"].Value = "Date of Birth";
                worksheet.Cells["D1"].Value = "Age";
                worksheet.Cells["E1"].Value = "Gender";
                worksheet.Cells["F1"].Value = "Country";
                worksheet.Cells["G1"].Value = "Address";
                worksheet.Cells["H1"].Value = "Receive News Letters";

                using(ExcelRange headerCells = worksheet.Cells["A1:H1"])
                {
                    headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerCells.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    headerCells.Style.Font.Bold = true;
                }

                int row = 2;
                List<PersonResponse> persons = _db.Persons.Include("Country").Select(p=>p.ToPersonResponse()).ToList();

                foreach (PersonResponse person in persons)
                {
                    worksheet.Cells[row, 1].Value = person.Name;
                    worksheet.Cells[row, 2].Value = person.Email;
                    if (person.DateOfBirth.HasValue)
                        worksheet.Cells[row, 3].Value = person.DateOfBirth.Value.ToString("yyyy-MMM-dd");
                    worksheet.Cells[row, 4].Value = person.Age;
                    worksheet.Cells[row, 5].Value = person.Gender;
                    worksheet.Cells[row, 6].Value = person.CountryName;
                    worksheet.Cells[row, 7].Value = person.Address;
                    worksheet.Cells[row, 8].Value = person.ReceiveNewsLetters;

                    row++;
                }

                worksheet.Cells[$"A1:H{row}"].AutoFitColumns();

                await excelPackage.SaveAsync();
            }

            stream.Position = 0;
            return stream;
        }
    }
}
