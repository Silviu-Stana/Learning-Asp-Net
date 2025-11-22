using Models.Models;
using Models.Interfaces;
using Services.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IFinnhubService, FinnhubService>();
builder.Services.AddSingleton<IStockService, StockService>();

builder.Services.Configure<SocialMediaLinksOptions>(builder.Configuration.GetSection("SocialMediaLinks"));
builder.Services.Configure<WeatherApiOptions>(builder.Configuration.GetSection("weatherapi"));


builder.Configuration.AddJsonFile("MyOwnConfig.json", optional: true, reloadOnChange: true);

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();


app.Configuration["MyKey"] = "NewValue";

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Run();