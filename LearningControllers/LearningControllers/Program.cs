
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(); //adds them all as services

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Run();
