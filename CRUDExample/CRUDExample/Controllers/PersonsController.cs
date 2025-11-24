using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
    [Route("persons")]
    public class PersonsController : Controller
    {
        private readonly IPersonService _personService;
        private readonly ICountriesService _countriesService;

        public PersonsController(IPersonService personService, ICountriesService countriesService)
        {
            _personService = personService;
            _countriesService = countriesService;
        }

        [Route("/")] //the / overrides the prefix route
        [Route("index")]
        public IActionResult Index(string? searchString, string searchBy, string sortBy = nameof(PersonResponse.Name), SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {
            //Search
            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                { nameof(PersonResponse.Name), "Person Name" },
                { nameof(PersonResponse.Email), "Email" },
                { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
                { nameof(PersonResponse.Gender), "Gender" },
                { nameof(PersonResponse.CountryName), "Country" },
                { nameof(PersonResponse.Address), "Address" },
            };

            //FILTER
            List<PersonResponse> persons = _personService.GetFilteredPersons(searchBy, searchString);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;


            //SORT
            List<PersonResponse> sortedPersons = _personService.GetPersonsSortedByName(persons, sortBy, sortOrder);

            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder.ToString();

            return View(sortedPersons);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            List<CountryResponse> countries = _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(c=>new SelectListItem() { Text= c.CountryName, Value=c.CountryID.ToString()});

            return View();
        }

        [HttpPost("create")]
        public IActionResult Create(PersonAddRequest personAddRequest)
        {
            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = _countriesService.GetAllCountries();
                ViewBag.Countries = countries;
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View();
            }

            PersonResponse personResponse = _personService.AddPerson(personAddRequest);
            return RedirectToAction("Index");
        }

        [HttpGet("edit/{personId}")]
        public IActionResult Edit(Guid personId)
        {
            List<CountryResponse> countries = _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(c => new SelectListItem() { Text = c.CountryName, Value = c.CountryID.ToString() });

            PersonResponse? person = _personService.GetPersonById(personId);

            if (person == null) return RedirectToAction("Index");

            PersonUpdateRequest updateRequest = person.ToPersonUpdateRequest();

            return View(updateRequest);
        }

        [HttpPost("edit/{personId}")]
        public IActionResult Edit(PersonUpdateRequest updateRequest)
        {
            PersonResponse? person = _personService.GetPersonById(updateRequest.PersonID);

            if (person == null) return RedirectToAction("Index");

            if (ModelState.IsValid)
            {
                PersonResponse updatedPerson = _personService.UpdatePerson(updateRequest);
                return RedirectToAction("Index");
            }
            else
            {
                List<CountryResponse> countries = _countriesService.GetAllCountries();
                ViewBag.Countries = countries;
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View(person.ToPersonUpdateRequest());
            }
        }


        [HttpGet("delete/{personId}")]
        public IActionResult Delete(Guid personId)
        {
            List<CountryResponse> countries = _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(c => new SelectListItem() { Text = c.CountryName, Value = c.CountryID.ToString() });

            PersonResponse? person = _personService.GetPersonById(personId);

            if (person == null) return RedirectToAction("Index");

            return View(person);
        }

        [HttpPost("delete/{personId}")]
        public IActionResult Delete(PersonUpdateRequest updateRequest)
        {
            PersonResponse? person = _personService.GetPersonById(updateRequest.PersonID);

            if (person == null) return RedirectToAction("Index");

            _personService.DeletePerson(updateRequest.PersonID);
            return RedirectToAction("Index");
        }
        }
}
