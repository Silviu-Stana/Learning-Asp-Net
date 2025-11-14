using Entities;
using System;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// DTO to return most Person Service methods
    /// </summary>
    public class PersonResponse
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public Guid? CountryID { get; set; }
        public string? CountryName { get; set; }
        public string? Address { get; set; }
        public bool? ReceiveNewsLetters { get; set; }
        public double? Age { get; set; }

        /// <summary>
        /// Compare current object data with the parameter object.
        /// </summary>
        /// <param name="obj">The PersonResponse object to compare to</param>
        /// <returns>Are all person details a match? Then <b>true</b>. Else <b>false</b>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            if (obj.GetType() != typeof(PersonResponse)) return false;

            PersonResponse person = (PersonResponse)obj;

            return Id == person.Id && Name==person.Name && Email ==person.Email &&
                DateOfBirth ==person.DateOfBirth && Gender ==person.Gender &&
                CountryID ==person.CountryID &&CountryName ==person.CountryName &&
                Address ==person.Address && ReceiveNewsLetters ==person.ReceiveNewsLetters && Age ==person.Age;
        }

        public override string ToString()
        {
            return $"Person Id: {Id}, Name: {Name}, Email: {Email}, DateOfBirth: {DateOfBirth?.ToString("dd MMM yyyy")}, Gender: {Gender}, Country ID: {CountryID}, Country: {CountryName}, ReceiveNewsletters: {ReceiveNewsLetters}";
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    public static class PersonExtensions
    {
        /// <summary>
        /// Extension method to convert Person to PersonResponse.
        /// </summary>
        /// <param name="person">The object to convert.</param>
        /// <returns>Converted PersonResponse object.</returns>
        public static PersonResponse ToPersonResponse(this Person person)
        {
            return new PersonResponse()
            {
                Id = person.Id,
                Name = person.Name,
                Email = person.Email,
                DateOfBirth = person.DateOfBirth,
                Gender = person.Gender,
                CountryID = person.CountryID,
                Address = person.Address,
                ReceiveNewsLetters = person.ReceiveNewsLetters,
                Age = (person.DateOfBirth != null) ? Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25) : null
            };
        }
    }
}
