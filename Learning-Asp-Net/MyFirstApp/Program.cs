using MyFirstApp.CustomMiddleware;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseBasicAuthAsignmentMiddleware();

app.Run();