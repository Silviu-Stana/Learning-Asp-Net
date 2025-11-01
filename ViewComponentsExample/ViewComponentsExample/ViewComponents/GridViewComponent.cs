using Microsoft.AspNetCore.Mvc;
using ViewComponentsExample.Models;

namespace ViewComponentsExample.ViewComponents
{
    public class GridViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(PersonGridModel grid)
        {
            //PersonGridModel personGridModel = new PersonGridModel()
            //{
            //    Title = "People List",
            //    People = {
            //        new(){Name="John", JobTitle="Engineer"},
            //        new(){Name="Jones", JobTitle="Manager"},
            //        new(){Name="William", JobTitle="CFO"}
            //    }
            //};

            return View("Sample", grid); //partial view, normally searched at: Views/Shared/Components/Grid/Default.cshtml
        }
    }
}
