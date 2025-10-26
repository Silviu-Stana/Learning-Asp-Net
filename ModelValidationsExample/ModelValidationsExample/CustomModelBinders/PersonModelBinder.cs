using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModelValidationsExample.Models;

namespace ModelValidationsExample.CustomModelBinders
{
    public class PersonModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            Person p = new();

            if(bindingContext.ValueProvider.GetValue("FirstName").Length > 0)
            {
                p.PersonName = bindingContext.ValueProvider.GetValue("FirstName").FirstValue;
            }

            if (bindingContext.ValueProvider.GetValue("LastName").Length > 0)
            {
                p.PersonName += " " + bindingContext.ValueProvider.GetValue("LastName").FirstValue;
            }


            //If we don't do this all values except name will be null (won't bind)
            if (bindingContext.ValueProvider.GetValue("Email").Length > 0) p.Email = bindingContext.ValueProvider.GetValue("Email").FirstValue;
            if (bindingContext.ValueProvider.GetValue("Phone").Length > 0) p.Phone = bindingContext.ValueProvider.GetValue("Phone").FirstValue;
            if (bindingContext.ValueProvider.GetValue("Password").Length > 0) p.Password = bindingContext.ValueProvider.GetValue("Password").FirstValue;
            if (bindingContext.ValueProvider.GetValue("ConfirmPassword").Length > 0) p.ConfirmPassword = bindingContext.ValueProvider.GetValue("ConfirmPassword").FirstValue;
            if (bindingContext.ValueProvider.GetValue("DateOfBirth").Length > 0) p.DateOfBirth = Convert.ToDateTime(bindingContext.ValueProvider.GetValue("DateOfBirth").FirstValue);
            if (bindingContext.ValueProvider.GetValue("Price").Length > 0) p.Price = Convert.ToDouble(bindingContext.ValueProvider.GetValue("Price").FirstValue);

            bindingContext.Result = ModelBindingResult.Success(p);

            return Task.CompletedTask;
        }
    }
}
