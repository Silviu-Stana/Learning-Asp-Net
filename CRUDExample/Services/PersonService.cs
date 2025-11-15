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
            
            ValidationHelper.ModelValidation(request);

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
            throw new NotImplementedException();
        }

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
    }
}
