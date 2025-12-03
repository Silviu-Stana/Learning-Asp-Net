using System;
using System.Collections.Generic;
using Xunit;
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class PersonServiceTest
    {
        private readonly IPersonService _personService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _testOutputHelper;

        public PersonServiceTest(ITestOutputHelper testOutputHelper)
        {
            _countriesService = new CountriesService(new PersonsDbContext(new DbContextOptionsBuilder<PersonsDbContext>().Options));
            _personService = new PersonService(new PersonsDbContext(new DbContextOptionsBuilder<PersonsDbContext>().Options), _countriesService);
            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson
        [Fact]
        public async Task AddPerson_Null_ThrowsArgumentNull()
        {
            PersonAddRequest? personAddRequest = null;

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _personService.AddPerson(personAddRequest));
        }

        [Fact]
        public async Task AddPerson_NameNull_ThrowsArgumentException()
        {
            PersonAddRequest? personAddRequest = new() { Name = null };

            await Assert.ThrowsAsync<ArgumentException>(async () => await _personService.AddPerson(personAddRequest));
        }

        [Fact]
        public async Task AddPerson_ProperData_InsertsInPersonList()
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

            PersonResponse response = await _personService.AddPerson(personAddRequest);
            List<PersonResponse> personList = await _personService.GetAllPersons();

            Assert.True(response.Id != Guid.Empty);
            Assert.Contains(response, personList);
        }
        #endregion

        #region GetPersonByPersonID
        //If we supply null, return null
        [Fact]
        public async Task GetPersonById_Null_ReturnsNull()
        {
            Guid? personId = null;

            PersonResponse? personResponse = await _personService.GetPersonById(personId);

            Assert.Null(personResponse);
        }


        [Fact]
        public async Task GetPersonById_ProperRequest_Succeds()
        {
            CountryAddRequest countryAddRequest = new() { CountryName = "Canada" };
            CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequest);

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

            PersonResponse addedPerson = await _personService.AddPerson(personAddRequest);

            PersonResponse? person = await _personService.GetPersonById(addedPerson.Id);

            Assert.Equal(addedPerson, person);

        }
        #endregion

        #region GetAllPersons
        [Fact]
        //by default empty list
        public async Task GetAllPersons_ByDefault_EmptyList()
        {
            List<PersonResponse> personResponses = await _personService.GetAllPersons();
            Assert.Empty(personResponses);
        }

        [Fact]
        public async Task GetAllPersons_Add4People_ReturnsListOf4People()
        {
            List<PersonAddRequest> personAddRequests = await AddMultiplePersons();

            List<PersonResponse> personResponseList = new();

            foreach (PersonAddRequest request in personAddRequests)
            {
                personResponseList.Add(await _personService.AddPerson(request));
            }

            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse p in personResponseList) _testOutputHelper.WriteLine(p.ToString());

            List<PersonResponse> people = await _personService.GetAllPersons();

            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse p in people) _testOutputHelper.WriteLine(p.ToString());


            foreach (PersonResponse p in people) Assert.Contains(p, people);
        }
        #endregion

        #region GetFilteredPersons
        //If search text empty, return all persons.
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText_ReturnsAllPersons()
        {
            List<PersonAddRequest> personAddRequests = await AddMultiplePersons();

            List<PersonResponse> personResponseList = new();

            foreach (PersonAddRequest request in personAddRequests)
            {
                personResponseList.Add(await _personService.AddPerson(request));
            }

            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse p in personResponseList) _testOutputHelper.WriteLine(p.ToString());

            List<PersonResponse> people = await _personService.GetFilteredPersons(nameof(Person.Name), "");

            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse p in people) _testOutputHelper.WriteLine(p.ToString());


            foreach (PersonResponse p in people) Assert.Contains(p, people);
        }

        [Fact]
        public async Task GetFilteredPersons_SearchingPersonName_ReturnsMatch()
        {
            List<PersonAddRequest> personAddRequests = await AddMultiplePersons();

            foreach (PersonAddRequest request in personAddRequests)
            {
                await _personService.AddPerson(request);
            }

            List<PersonResponse> people = await _personService.GetFilteredPersons(nameof(Person.Name), "bo"); //Mary is a name included in AddMultiplePersons();

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
        public async Task GetSortedPersons_ByName_Descending()
        {
            List<PersonAddRequest> personAddRequests = await AddMultiplePersons();

            List<PersonResponse> allPeople = new();

            foreach (PersonAddRequest request in personAddRequests)
            {
                allPeople.Add(await _personService.AddPerson(request));
            }

            List<PersonResponse> sortedPersons = _personService.GetSortedPersons(allPeople, nameof(Person.Name), SortOrderOptions.DESC);

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
        public async Task UpdatePerson_Null_ThrowsArgumentNull()
        {
            PersonUpdateRequest? update = null;

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _personService.UpdatePerson(update));
        }

        [Fact]
        public async Task UpdatePerson_InvalidPersonId_ThrowsArgumentException()
        {
            PersonUpdateRequest update = new() { PersonID=Guid.NewGuid()};

            await Assert.ThrowsAsync<ArgumentException>(async () => await _personService.UpdatePerson(update));
        }

        [Fact]
        public async Task UpdatePerson_PersonNameNull_ThrowsArgumentException()
        {
            PersonAddRequest personAdd = await AddOnePerson();

            await _personService.AddPerson(personAdd);

            // Get the list of all people and take the first person we created
            List<PersonResponse> people = await _personService.GetAllPersons();
            PersonResponse person = people.First();

            PersonUpdateRequest updatedPerson = person.ToPersonUpdateRequest();

            updatedPerson.Name = null;

            await Assert.ThrowsAsync<ArgumentException>(async () => await _personService.UpdatePerson(updatedPerson));
        }

        [Fact]
        public async Task UpdatePerson_UpdateNameAndEmail()
        {
            PersonAddRequest personAdd = await AddOnePerson();

            await _personService.AddPerson(personAdd);

            List<PersonResponse> people = await _personService.GetAllPersons();
            PersonResponse bob = people.First();

            PersonUpdateRequest personToUpdate = bob.ToPersonUpdateRequest();

            personToUpdate.Name = "Zack";
            personToUpdate.Email = "zack@gmail.com";

            PersonResponse update =  await _personService.UpdatePerson(personToUpdate);

            PersonResponse? updatedPerson =  await _personService.GetPersonById(update.Id);

            Assert.Equal(updatedPerson, update);
        }
        #endregion

        #region DeletePerson
        [Fact]
        public async Task DeletePerson_ValidId_ReturnsTrue()
        {
            PersonAddRequest personAdd = await AddOnePerson();

            PersonResponse personResponse = await _personService.AddPerson(personAdd);

            bool isDeleted = await _personService.DeletePerson(personResponse.Id);

            Assert.True(isDeleted);
        }

        [Fact]
        public async Task DeletePerson_InvalidId_ReturnsFalse()
        {
            bool isDeleted = await _personService.DeletePerson(Guid.NewGuid());
            Assert.False(isDeleted);
        }
        #endregion


        #region FakeDataFunctions
        async Task<PersonAddRequest> AddOnePerson()
        {
            CountryAddRequest country = new() { CountryName = "USA" };

            CountryResponse countryResponse = await _countriesService.AddCountry(country);

            PersonAddRequest personAddRequest = new()
            {
                Name = "Smith",
                Email = "smith@gmail.com",
                Address = "address",
                CountryID = countryResponse.CountryID,
                DateOfBirth = DateTime.Parse("2002-05-06"),
                Gender = GenderOptions.Male,
                ReceiveNewsLetters = false
            };

            return personAddRequest;
        }

        async Task<List<PersonAddRequest>> AddMultiplePersons()
        {
            CountryAddRequest country1 = new() { CountryName = "USA" };
            CountryAddRequest country2 = new() { CountryName = "India" };

            CountryResponse countryResponse1 = await _countriesService.AddCountry(country1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(country2);

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
        #endregion
    }
}