using System;
using System.Collections.Generic;
using Xunit;
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;

namespace Tests
{
    public class PersonServiceTest
    {
        private readonly IPersonService _personService;

        public PersonServiceTest()
        {
            _personService = new PersonService();
        }

        #region AddPerson
        [Fact]
        public void AddPerson_Null_ThrowsArgumentNull()
        {
            PersonAddRequest? personAddRequest = null;

            Assert.Throws<ArgumentNullException>(() =>_personService.AddPerson(personAddRequest));
        }

        [Fact]
        public void AddPerson_NameNull_ThrowsArgumentException()
        {
            PersonAddRequest? personAddRequest = new() { Name=null};

            Assert.Throws<ArgumentException>(() => _personService.AddPerson(personAddRequest));
        }
        #endregion
    }
}
