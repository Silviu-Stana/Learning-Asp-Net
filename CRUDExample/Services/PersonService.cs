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

        public PersonService(bool initialize = true)
        {
            _persons = new List<Person>();
            _countriesService = new CountriesService();

            if (initialize)
            {

                // Mock Data: List<Person>
                _persons.AddRange([
                    new () {Id=Guid.Parse("D3FC1518-FA6F-429F-A335-0075E1DDA011"), Name="Mary", Email="mary@gmail.com", DateOfBirth=DateTime.Parse("1993-01-02"), Gender="Male", Address= "6 Lerdahl Alley", ReceiveNewsLetters=false, CountryID=Guid.Parse("FAE1E3D1-3FEA-47C8-A3E0-095E827E1170")},
                    new () {Id=Guid.Parse("6E3A9C8D-0F2B-45A0-9E1D-3C4F5A6B7C8E"), Name="John Doe", Email="john.doe@example.com", DateOfBirth=DateTime.Parse("1985-11-20"), Gender="Male", Address= "123 Main St", ReceiveNewsLetters=true, CountryID=Guid.Parse("15A1F30A-BC16-4CEF-9AFC-137E41EE7812")},
                    new () {Id=Guid.Parse("A1B2C3D4-E5F6-7A8B-9C0D-1E2F3A4B5C6D"), Name="Jane Smith", Email="jane.smith@sample.org", DateOfBirth=DateTime.Parse("2001-05-15"), Gender="Female", Address= "45 Oak Ave", ReceiveNewsLetters=false, CountryID=Guid.Parse("FC3F2A77-7CA3-4C2E-A8BC-10C1E52F9210")},
                    new () {Id=Guid.Parse("F5E4D3C2-B1A0-9D8C-7B6A-5F4E3D2C1B0A"), Name="Robert Brown", Email="robert.b@web.net", DateOfBirth=DateTime.Parse("1970-08-25"), Gender="Male", Address= "78 Maple Lane", ReceiveNewsLetters=true, CountryID=Guid.Parse("FAE1E3D1-3FEA-47C8-A3E0-095E827E1170")},
                    new () {Id=Guid.Parse("C9B8A7F6-E5D4-C3B2-A190-8F7E6D5C4B3A"), Name="Emily Davis", Email="emily.d@mail.com", DateOfBirth=DateTime.Parse("1998-03-10"), Gender="Female", Address= "10 Pine Rd", ReceiveNewsLetters=false, CountryID=Guid.Parse("B06A3323-C6EF-4073-89BC-43942764EBC5")},
                    new () {Id=Guid.Parse("3B4C5D6E-7F8A-9B0C-1D2E-3F4A5B6C7D8E"), Name="Michael Wilson", Email="michael.w@outlook.com", DateOfBirth=DateTime.Parse("1965-12-01"), Gender="Male", Address= "22 Elm Plaza", ReceiveNewsLetters=true, CountryID=Guid.Parse("15A1F30A-BC16-4CEF-9AFC-137E41EE7812")},
                    new () {Id=Guid.Parse("7C8B9A0D-1E2F-3C4D-5E6F-7A8B9C0D1E2F"), Name="Jessica Lee", Email="jessica.l@provider.co", DateOfBirth=DateTime.Parse("1995-07-30"), Gender="Female", Address= "33 Poplar Hts", ReceiveNewsLetters=false, CountryID=Guid.Parse("FC3F2A77-7CA3-4C2E-A8BC-10C1E52F9210")}
                ]);
            }
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
            return _persons.Select(person => ConvertPersonToPersonResponse(person)).ToList();
        }

        public PersonResponse? GetPersonById(Guid? personId)
        {
            if (personId == null) return null;

            Person? person = _persons.FirstOrDefault(x => x.Id == personId);
            
            if (person == null) return null;

            return ConvertPersonToPersonResponse(person);
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

        public List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString)
        {
            List<PersonResponse> all = GetAllPersons();
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

            return ConvertPersonToPersonResponse(match);
        }

        public bool DeletePerson(Guid? personId)
        {
            if (personId == null) throw new ArgumentNullException(nameof(personId));

            Person? person = _persons.FirstOrDefault(p => p.Id == personId);

            if(person == null) return false;

            _persons.RemoveAll(p=>p.Id == personId);

            return true;
        }
    }
}
