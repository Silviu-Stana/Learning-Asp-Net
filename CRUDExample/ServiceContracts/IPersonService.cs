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

        /// <summary>
        /// Returns person object based on ID.
        /// </summary>
        /// <param name="personId"></param>
        /// <returns>Returns a match, if it exists.</returns>
        PersonResponse? GetPersonById(Guid? personId);

        /// <summary>
        /// Return all person objects that match the given search field, and search string.
        /// </summary>
        /// <param name="searchBy">Search field to use</param>
        /// <param name="searchString">String to search</param>
        /// <returns></returns>
        List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString);
    }
}
