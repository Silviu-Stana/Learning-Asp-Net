using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

/// <summary>
/// HOW TO USE: app.UseCalculator();
/// </summary>
public class CalculatorMiddleware
{
    private readonly RequestDelegate _next;

    public CalculatorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/calculate") // e.g., run only on /calculate route
        {
            if (context.Request.Method != "GET")
            {
                await context.Response.WriteAsync("Must be a GET request\n");
                return;
            }

            string? firstNumber;
            string? secondNumber;
            string? operation;

            int number1 = 0, number2 = 0;

            if (context.Request.Query.ContainsKey("firstNumber"))
            {
                firstNumber = context.Request.Query["firstNumber"];
                if (firstNumber != null && IsNumber(firstNumber))
                    number1 = int.Parse(firstNumber);
                else
                    await context.Response.WriteAsync("Invalid input for 'firstNumber'\n");
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Missing 'firstNumber'\n");
                return;
            }

            if (context.Request.Query.ContainsKey("secondNumber"))
            {
                secondNumber = context.Request.Query["secondNumber"];
                if (secondNumber != null && IsNumber(secondNumber))
                    number2 = int.Parse(secondNumber);
                else
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid input for 'secondNumber'\n");
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Missing 'secondNumber'\n");
                return;
            }

            if (context.Request.Query.ContainsKey("operation"))
            {
                operation = context.Request.Query["operation"];
                if (!IsOperationValid(operation!))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid input for 'operation'\n");
                    return;
                }

                string result = operation switch
                {
                    "+" => $"{number1}+{number2}={number1 + number2}",
                    "-" => $"{number1}-{number2}={number1 - number2}",
                    "*" => $"{number1}*{number2}={number1 * number2}",
                    "/" => $"{number1}/{number2}={number1 / number2}",
                    "%" => $"{number1}%{number2}={number1 % number2}",
                    _ => "Unsupported operation"
                };

                await context.Response.WriteAsync(result);
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Missing 'operation'\n");
            }

            return; // Stop pipeline
        }

        await _next(context); // Continue to next middleware
    }

    static private bool IsNumber(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        return int.TryParse(value.Trim(), out _);
    }

    static bool IsOperationValid(string operation)
    {
        //must send "%2B"  with postman for valid "+" symbol
        if (string.IsNullOrEmpty(operation)) return false;
        string allowedOperations = "+-/*%";
        string opTrim = operation.Trim();
        if (opTrim.Length == 1 && allowedOperations.IndexOf(opTrim[0]) >= 0) return true;

        return false;
    }
}
