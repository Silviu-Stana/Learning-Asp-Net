using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;

namespace MyFirstApp.CustomMiddleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class BasicAuthAsignmentMiddleware
    {
        private readonly RequestDelegate _next;

        public BasicAuthAsignmentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string method = context.Request.Method;
            string path = context.Request.Path;

            bool validEmail=false, validPassword=false;

            StreamReader reader = new StreamReader(context.Request.Body);
            string body = await reader.ReadToEndAsync();
            Dictionary<string, StringValues> query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(body);

            if (method == "POST" && path == "/")
            {
                if (query.ContainsKey("email"))
                {
                    if (query["email"] != "admin@example.com")
                    {
                        InvalidLogin(context);
                        return;
                    }
                    else validEmail = true;
                }
                else
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid input for 'email'");
                }

                if (query.ContainsKey("password"))
                {
                    if (query["password"] != "admin1234")
                    {
                        InvalidLogin(context);
                        return;
                    }
                    else validPassword = true;
                }
                else
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid input for 'password'");
                }

                if (validEmail && validPassword)
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Successful login");
                }
            }

            if (method == "GET" && path == "/")
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("No response");
            }

            await _next(context);
        }
       

        async void InvalidLogin(HttpContext context)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid login");
        }
    }
}

namespace MyFirstApp.CustomMiddleware
{
    public static class BasicAuthAsignmentMiddlewareExtensions
    {
        public static IApplicationBuilder UseBasicAuthAsignmentMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthAsignmentMiddleware>();
        }
    }
}


//ASSIGNMENT INSTRUCTIONS:
/*
 * Requirement: Create an Asp.Net Core Web Application that receives username and password via POST request (from Postman).

It receives "email" and "password" as query string from request body.



Parameters:
email: any email address
password: any password string
Finally, it should return message as either "Successful login" or "Invalid login".



Process:
If email is "admin@example.com" and password is "admin1234", it is treated as a valid login; otherwise invalid login.



Example #1:
If you receive a HTTP POST request at path "/", if the valid email and password are submitted, it should return HTTP 200 response.

Request Url: /
Request Method: POST
Request body (input): email=admin@example.com&password=admin1234
Response Status Code: 200
Response body (output):
Successful login




Example #2:

If you receive a HTTP POST request at path "/", if either email or password is incorrect, it should return HTTP 400 response.
Request Url: /
Request Method: POST
Request body (input): email=manager@example.com&password=manager-password

Response Status Code: 400

Response body (output):

Invalid login



Example #3:

If you receive a HTTP POST request at path "/", if neither email and password is submitted, it should return HTTP 400 response.

Request Url: /

Request Method: POST

Request body (input): [empty]

Response Status Code: 400

Response body (output):

Invalid input for 'email'
Invalid input for 'password'



Example #4:

If you receive a HTTP POST request at path "/", if password is not submitted, it should return HTTP 400 response.

Request Url: /

Request Method: POST

Request body (input): email=test@example.com

Response Status Code: 400

Response body (output):

Invalid input for 'password'



Example #5:

If you receive a HTTP POST request at path "/", if email not is submitted, it should return HTTP 400 response.

Request Url: /

Request Method: POST

Request body (input): password=1234

Response Status Code: 400

Response body (output):

Invalid input for 'password'



Example #6:

If you receive a HTTP GET request at path "/", it should return HTTP 200 response.

Request Url: /
Request Method: GET
Response Status Code: 200
Response body (output):
No response





Instructions:

Use custom conventional middleware (with middleware extensions) to handle the post request at path "/"

The "email" and "password" values are mandatory

Return appropriate HTTP status codes based on above examples.

Do not create controllers or any other concept which is not yet covered, to avoid confusion.


 * 
 */