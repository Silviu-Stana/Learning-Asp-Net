using System;
using System.ComponentModel.DataAnnotations;
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
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
            throw new NotImplementedException();
        }
    }
}
