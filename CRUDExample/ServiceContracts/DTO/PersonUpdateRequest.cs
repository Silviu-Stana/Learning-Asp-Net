using Entities;
using ServiceContracts.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// DTO  for updating a new person
    /// </summary>
    public class PersonUpdateRequest
    {
        [Required(ErrorMessage ="Person ID can't be blank.")]
        public Guid PersonID { get; set; }

        [Required(ErrorMessage = "Person Name cannot be blank.")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Email cannot be blank.")]
        [EmailAddress]
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public GenderOptions? Gender { get; set; }
        public Guid? CountryID { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }

        /// <summary>
        /// Convert add request to new Person object.
        /// </summary>
        /// <returns></returns>
        public Person ToPerson()
        {
            return new Person()
            {
                Id = PersonID,
                Name = Name,
                Email = Email,
                DateOfBirth = DateOfBirth,
                Gender = Gender.ToString(),
                Address = Address,
                ReceiveNewsLetters = ReceiveNewsLetters
            };
        }
    }
}
