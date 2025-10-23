
using System.Text.RegularExpressions;

namespace MyFirstApp.CustomConstraints
{

    //Eg: sales-report/2040/apr
    public class MonthsCustomConstraint : IRouteConstraint
    {
        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            //check whether the value exists
            if (!values.ContainsKey(routeKey)) return false;

            Regex regex = new Regex($"^(apr|jan|oct|jul)$");
            string? monthValue = Convert.ToString(values[routeKey]);

            if(regex.IsMatch(monthValue!)) return true;

            return false;
        }
    }
}
