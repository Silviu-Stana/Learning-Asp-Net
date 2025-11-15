using System;
using System.ComponentModel.DataAnnotations;
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;

namespace Services
{
    public class PersonService : IPersonService
    {
        private readonly List<Person> _persons;
        private readonly ICountriesService _countriesService;

        public PersonService()
        {
            _persons = [];
            _countriesService = new CountriesService();
        }

        private PersonResponse ConvertPersonToPersonResponse(Person person)
        {
            PersonResponse personResponse = person.ToPersonResponse();
            personResponse.CountryName = _countriesService.GetCountryById(person.CountryID)?.CountryName;

            return personResponse;
        }

        public PersonResponse AddPerson(PersonAddRequest? request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            
            ValidationHelper.CheckForValidationErrors(request);

            Person person = request.ToPerson();

            person.Id = Guid.NewGuid();


            _persons.Add(person);

            return ConvertPersonToPersonResponse(person);
        }

        public List<PersonResponse> GetAllPersons()
        {
            return _persons.Select(person => person.ToPersonResponse()).ToList();
        }

        public PersonResponse? GetPersonById(Guid? personId)
        {
            if (personId == null) return null;

            Person? person = _persons.FirstOrDefault(x => x.Id == personId);
            
            if (person == null) return null;

            return person.ToPersonResponse();
        }

        public List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString)
        {
            List<PersonResponse> all = GetAllPersons();
            List<PersonResponse> matchingPersons = all;

            if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString)) return matchingPersons;

            switch (searchBy)
            {
                case nameof(Person.Name):
                    matchingPersons = FilterByName(all, searchString);
                    break;
                case nameof(Person.DateOfBirth):
                    matchingPersons = FilterByDateOfBirth(all, searchString);
                    break;
                case nameof(Person.Gender):
                    matchingPersons = FilterByGender(all, searchString);
                    break;
                case nameof(Person.CountryID):
                    matchingPersons = FilterByCountryID(all, searchString);
                    break;
                case nameof(Person.Address):
                    matchingPersons = FilterByAddress(all, searchString);
                    break;
                default:
                    matchingPersons = all;
                    break;
            }

            return matchingPersons;
        }

        public List<PersonResponse> GetPersonsSortedByName(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
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

        public PersonResponse UpdatePerson(PersonUpdateRequest? request)
        {
            if (request == null) throw new ArgumentNullException(nameof(Person));

            ValidationHelper.CheckForValidationErrors(request);

            Person? match = _persons.FirstOrDefault(p => p.Id == request.PersonID);
            if (match == null) throw new ArgumentException("Given Person ID does not exists");

            //Update
            match.Name = request.Name;
            match.Email = request.Email;
            match.Address = request.Address;
            match.Gender = request.Gender.ToString();
            match.DateOfBirth = request.DateOfBirth;
            match.CountryID = request.CountryID;
            match.ReceiveNewsLetters = request.ReceiveNewsLetters;

            return match.ToPersonResponse();
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

        private List<PersonResponse> FilterByCountryID(List<PersonResponse> allPeople, string searchString)
        {
            return allPeople.Where(p => p.CountryID == null || p.CountryID.Value.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private List<PersonResponse> FilterByAddress(List<PersonResponse> allPeople, string searchString)
        {
            return allPeople.Where(p => !string.IsNullOrEmpty(p.Address) ? p.Address.Contains(searchString, StringComparison.OrdinalIgnoreCase) : true).ToList();
        }


        #endregion
    }
}
