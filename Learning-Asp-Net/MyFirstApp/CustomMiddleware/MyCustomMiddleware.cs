
namespace MyFirstApp.CustomMiddleware
{
    public class MyCustomMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await context.Response.WriteAsync("Start");
            await next(context);
            await context.Response.WriteAsync("End");
        }
    }
}
