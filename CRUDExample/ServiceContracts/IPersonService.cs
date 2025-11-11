using System;
using ServiceContracts.DTO;

namespace ServiceContracts
{
    /// <summary>
    /// Business logic for manipulating Person entity.
    /// </summary>
    public interface IPersonService
    {
        /// <summary>
        /// Adds person into existing List<Person>
        /// </summary>
        /// <param name="request">Person to add</param>
        /// <returns>The same person details.</returns>
        PersonResponse AddPerson(PersonAddRequest? request);

        /// <summary>
        /// Retrieves a list of all persons.
        /// </summary>
        /// <returns>A list of <see cref="PersonResponse"/> objects representing all persons.</returns>
        List<PersonResponse> GetAllPersons();
    }
}
