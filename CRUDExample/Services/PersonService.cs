using System;
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class PersonService : IPersonService
    {
        public PersonResponse AddPerson(PersonAddRequest? request)
        {
            throw new NotImplementedException();
        }

        public List<PersonResponse> GetAllPersons()
        {
            throw new NotImplementedException();
        }
    }
}
