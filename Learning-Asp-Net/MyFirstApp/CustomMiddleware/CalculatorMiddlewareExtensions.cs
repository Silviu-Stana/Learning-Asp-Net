using Microsoft.AspNetCore.Builder;

namespace MyFirstApp.CustomMiddleware
{
    public static class CalculatorMiddlewareExtensions
    {
        public static IApplicationBuilder UseCalculator(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CalculatorMiddleware>();
        }
    }
}
