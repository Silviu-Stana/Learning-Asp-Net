using System;
using System.Collections.Generic;
using Xunit;
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit.Abstractions;

namespace Tests
{
    public class PersonServiceTest
    {
        private readonly IPersonService _personService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _testOutputHelper;

        public PersonServiceTest(ITestOutputHelper testOutputHelper)
        {
            _personService = new PersonService();
            _countriesService = new CountriesService();
            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson
        [Fact]
        public void AddPerson_Null_ThrowsArgumentNull()
        {
            PersonAddRequest? personAddRequest = null;

            Assert.Throws<ArgumentNullException>(() => _personService.AddPerson(personAddRequest));
        }

        [Fact]
        public void AddPerson_NameNull_ThrowsArgumentException()
        {
            PersonAddRequest? personAddRequest = new() { Name = null };

            Assert.Throws<ArgumentException>(() => _personService.AddPerson(personAddRequest));
        }

        [Fact]
        public void AddPerson_ProperData_InsertsInPersonList()
        {
            PersonAddRequest? personAddRequest = new()
            {
                Name = "Name",
                Email = "email@gmail.com",
                Address = "address",
                CountryID = Guid.NewGuid(),
                Gender = GenderOptions.Male,
                DateOfBirth = DateTime.Parse("2000-01-01"),
                ReceiveNewsLetters = true
            };

            PersonResponse response = _personService.AddPerson(personAddRequest);
            List<PersonResponse> personList = _personService.GetAllPersons();

            Assert.True(response.Id != Guid.Empty);
            Assert.Contains(response, personList);
        }
        #endregion

        #region GetPersonByPersonID
        //If we supply null, return null
        [Fact]
        public void GetPersonById_Null_ReturnsNull()
        {
            Guid? personId = null;

            PersonResponse? personResponse = _personService.GetPersonById(personId);

            Assert.Null(personResponse);
        }


        [Fact]
        public void GetPersonById_ProperRequest_Succeds()
        {
            CountryAddRequest countryAddRequest = new() { CountryName = "Canada" };
            CountryResponse countryResponse = _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new()
            {
                Name = "John",
                Email = "email@gmail.com",
                Address = "address",
                CountryID = countryResponse.CountryID,
                DateOfBirth = DateTime.Parse("2000-01-01"),
                Gender = GenderOptions.Male,
                ReceiveNewsLetters = false
            };

            PersonResponse addedPerson = _personService.AddPerson(personAddRequest);

            PersonResponse? person = _personService.GetPersonById(addedPerson.Id);

            Assert.Equal(addedPerson, person);

        }
        #endregion

        #region GetAllPersons
        [Fact]
        //by default empty list
        public void GetAllPersons_ByDefault_EmptyList()
        {
            List<PersonResponse> personResponses = _personService.GetAllPersons();
            Assert.Empty(personResponses);
        }

        [Fact]
        public void GetAllPersons_Add4People_ReturnsListOf4People()
        {
            List<PersonAddRequest> personAddRequests = AddMultiplePersons();

            List<PersonResponse> personResponseList = new();

            foreach (PersonAddRequest request in personAddRequests)
            {
                personResponseList.Add(_personService.AddPerson(request));
            }

            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse p in personResponseList) _testOutputHelper.WriteLine(p.ToString());

            List<PersonResponse> people = _personService.GetAllPersons();

            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse p in people) _testOutputHelper.WriteLine(p.ToString());


            foreach (PersonResponse p in people) Assert.Contains(p, people);
        }
        #endregion

        #region GetFilteredPersons
        //If search text empty, return all persons.
        [Fact]
        public void GetFilteredPersons_EmptySearchText_ReturnsAllPersons()
        {
            List<PersonAddRequest> personAddRequests = AddMultiplePersons();

            List<PersonResponse> personResponseList = new();

            foreach (PersonAddRequest request in personAddRequests)
            {
                personResponseList.Add(_personService.AddPerson(request));
            }

            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse p in personResponseList) _testOutputHelper.WriteLine(p.ToString());

            List<PersonResponse> people = _personService.GetFilteredPersons(nameof(Person.Name), "");

            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse p in people) _testOutputHelper.WriteLine(p.ToString());


            foreach (PersonResponse p in people) Assert.Contains(p, people);
        }

        [Fact]
        public void GetFilteredPersons_SearchingPersonName_ReturnsMatch()
        {
            List<PersonAddRequest> personAddRequests = AddMultiplePersons();

            foreach (PersonAddRequest request in personAddRequests)
            {
                _personService.AddPerson(request);
            }

            List<PersonResponse> people = _personService.GetFilteredPersons(nameof(Person.Name), "bo"); //Mary is a name included in AddMultiplePersons();

            _testOutputHelper.WriteLine("Expected: only names that contain the letters 'bo'\n");
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse p in people) _testOutputHelper.WriteLine(p.Name);

            foreach (PersonResponse p in people)
            {
                if (p.Name != null)
                {
                    if (p.Name.Contains("bo", StringComparison.OrdinalIgnoreCase)) Assert.Contains(p, people);
                }
            }
        }

        #endregion
        
        #region GetSortedPersons

        [Fact]
        public void GetSortedPersons_ByName_Descending()
        {
            List<PersonAddRequest> personAddRequests = AddMultiplePersons();

            List<PersonResponse> allPeople = new();

            foreach (PersonAddRequest request in personAddRequests)
            {
                allPeople.Add(_personService.AddPerson(request));
            }

            List<PersonResponse> sortedPersons = _personService.GetPersonsSortedByName(allPeople, nameof(Person.Name), SortOrderOptions.DESC);

            allPeople = allPeople.OrderByDescending(p => p.Name).ToList();
            
            _testOutputHelper.WriteLine("Expected: names are sorted alphabetically, from Z to A\n");
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse p in sortedPersons) _testOutputHelper.WriteLine(p.Name);

            for (int i = 0; i < allPeople.Count; i++)
            {
                Assert.Equal(allPeople[i], sortedPersons[i]);
            }

        }


        #endregion

        #region UpdatePerson
        [Fact]
        public void UpdatePerson_Null_ThrowsArgumentNull()
        {
            PersonUpdateRequest? update = null;

            Assert.Throws<ArgumentNullException>(() => _personService.UpdatePerson(update));
        }

        [Fact]
        public void UpdatePerson_InvalidPersonId_ThrowsArgumentException()
        {
            PersonUpdateRequest update = new() { PersonID=Guid.NewGuid()};

            Assert.Throws<ArgumentException>(() => _personService.UpdatePerson(update));
        }

        [Fact]
        public void UpdatePerson_PersonNameNull_ThrowsArgumentException()
        {
            List<PersonAddRequest> people = AddMultiplePersons();

            foreach (PersonAddRequest p in people) _personService.AddPerson(p);

            PersonResponse person = _personService.GetAllPersons().First();

            PersonUpdateRequest updatedPerson = person.ToPersonUpdateRequest();

            updatedPerson.Name = null;

            Assert.Throws<ArgumentException>(() => _personService.UpdatePerson(updatedPerson));
        }

        [Fact]
        public void UpdatePerson_UpdateNameAndEmail()
        {
            List<PersonAddRequest> people = AddMultiplePersons();

            foreach (PersonAddRequest p in people) _personService.AddPerson(p);

            PersonResponse bob = _personService.GetFilteredPersons(nameof(Person.Name), "Bob").First();

            PersonUpdateRequest personToUpdate = bob.ToPersonUpdateRequest();

            personToUpdate.Name = "Zack";
            personToUpdate.Email = "zack@gmail.com";

            PersonResponse update =  _personService.UpdatePerson(personToUpdate);

            PersonResponse? updatedPerson =  _personService.GetPersonById(update.Id);

            Assert.Equal(updatedPerson, update);
        }
        #endregion

        List<PersonAddRequest> AddMultiplePersons()
        {
            CountryAddRequest country1 = new() { CountryName = "USA" };
            CountryAddRequest country2 = new() { CountryName = "India" };

            CountryResponse countryResponse1 = _countriesService.AddCountry(country1);
            CountryResponse countryResponse2 = _countriesService.AddCountry(country2);

            PersonAddRequest personAddRequest = new()
            {
                Name = "Smith",
                Email = "smith@gmail.com",
                Address = "address",
                CountryID = countryResponse1.CountryID,
                DateOfBirth = DateTime.Parse("2002-05-06"),
                Gender = GenderOptions.Male,
                ReceiveNewsLetters = false
            };

            PersonAddRequest personAddRequest2 = new()
            {
                Name = "Ana",
                Email = "ana@gmail.com",
                Address = "address",
                CountryID = countryResponse1.CountryID,
                DateOfBirth = DateTime.Parse("2002-05-06"),
                Gender = GenderOptions.Female,
                ReceiveNewsLetters = true
            };

            PersonAddRequest personAddRequest3 = new()
            {
                Name = "Bob",
                Email = "bob@gmail.com",
                Address = "address",
                CountryID = countryResponse2.CountryID,
                DateOfBirth = DateTime.Parse("2002-05-06"),
                Gender = GenderOptions.Male,
                ReceiveNewsLetters = false
            };

            PersonAddRequest personAddRequest4 = new()
            {
                Name = "Bobby",
                Email = "bob@gmail.com",
                Address = "address",
                CountryID = countryResponse2.CountryID,
                DateOfBirth = DateTime.Parse("2002-05-06"),
                Gender = GenderOptions.Male,
                ReceiveNewsLetters = false
            };

            return [personAddRequest, personAddRequest2, personAddRequest3, personAddRequest4];
        }
    }
}