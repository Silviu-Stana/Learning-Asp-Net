using System;
using System.ComponentModel.DataAnnotations;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;

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
    }
}
