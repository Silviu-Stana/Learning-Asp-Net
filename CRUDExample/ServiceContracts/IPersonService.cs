using System;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

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
        Task<PersonResponse> AddPerson(PersonAddRequest? request);

        /// <summary>
        /// Retrieves a list of all persons.
        /// </summary>
        /// <returns>A list of <see cref="PersonResponse"/> objects representing all persons.</returns>
        Task<List<PersonResponse>> GetAllPersons();

        /// <summary>
        /// Returns person object based on ID.
        /// </summary>
        /// <param name="personId"></param>
        /// <returns>Returns a match, if it exists.</returns>
        Task<PersonResponse?> GetPersonById(Guid? personId);

        /// <summary>
        /// Return all person objects that match the given search field, and search string.
        /// </summary>
        /// <param name="searchBy">Search field to use</param>
        /// <param name="searchString">String to search</param>
        /// <returns></returns>
        Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString);

        /// <summary>
        /// Returns list of ordered persons.
        /// </summary>
        /// <param name="allPersons">The full unordered list.</param>
        /// <param name="sortBy">Property name to sort by.</param>
        /// <param name="sortOrder">Ascending or descending.</param>
        /// <returns>An ordered person list.</returns>
        List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, string sortBy,
            SortOrderOptions sortOrder);

        /// <summary>
        /// Updates the specified person details based on the given person ID
        /// </summary>
        /// <param name="personUpdateRequest">Person details to update, including person ID</param>
        /// <returns>Updated PersonResponse object</returns>
        Task<PersonResponse> UpdatePerson(PersonUpdateRequest? request);

        /// <summary>
        /// Deletes person with given id.
        /// </summary>
        /// <param name="personId">Id to delete.</param>
        /// <returns>True if deletion is successful.</returns>
        Task<bool> DeletePerson(Guid? personId);

        /// <summary>
        /// Returns persons as CSV
        /// </summary>
        /// <returns>The memory stream with CSV data</returns>
        Task<MemoryStream> GetPersonsCSV();

        /// <summary>
        /// Returns persons as Excel
        /// </summary>
        /// <returns>Memory stream with excel data of persons</returns>
        Task<MemoryStream> GetPersonsExcel();
    }
}
