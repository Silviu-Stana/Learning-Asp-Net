using MyFirstApp;
using MyFirstApp.CustomMiddleware;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<MyCustomMiddleware>();

var app = builder.Build();

app.UseCalculator();

//Middleware 1
app.Use(async (HttpContext context, RequestDelegate next)=>{
    await context.Response.WriteAsync("Middleware 1\n");
    await next(context);
});

//Middleware 2
app.UseMyCustomMiddleware();

//Middleware 3
app.Run(async (HttpContext context) => {
    await context.Response.WriteAsync("Middleware 3\n");
});

app.Run();


Dictionary<int,string> employees = new Dictionary<int, string>()
{
    {101,"Scott" },
    {102,"Scott" },
    {103,"Scott" },
};

foreach (KeyValuePair<int,string> e in employees)
{
    Console.WriteLine(e.Key + ", " + e.Value);
}

Dictionary<int,string>.KeyCollection keys = employees.Keys;

public delegate int Comparison<in T>(T left, T right);

class Wow<T>
{
    public Comparison<T> comparator;
}