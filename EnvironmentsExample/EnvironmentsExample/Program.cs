using EnvironmentsExample;
using EnvironmentsExample.ServiceContracts;
using EnvironmentsExample.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IFinnhubService, FinnhubService>();

builder.Services.Configure<WeatherApiOptions>(builder.Configuration.GetSection("weatherapi"));


builder.Configuration.AddJsonFile("MyOwnConfig.json", optional: true, reloadOnChange: true);

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

app.Environment.IsEnvironment("Beta");


app.Configuration["MyKey"] = "NewValue";

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Run();