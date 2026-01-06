using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Application.Services;
using Application.Scrapers;
using Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure DbContext with SQL Server
builder.Services.AddDbContext<WebScraperDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Register application services
builder.Services.AddHttpClient<ImageDownloader>();
builder.Services.AddScoped<ImageDownloader>();

// Register default scraper (Olx)
builder.Services.AddScoped<IScraper, OlxScraper>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Clear Listings and Images on startup when in Development to ensure a clean DB after rebuilds
using (var scope = app.Services.CreateScope())
{
    try
    {
        var env = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        if (env.IsDevelopment())
        {
            var db = scope.ServiceProvider.GetRequiredService<WebScraperDbContext>();
            // Remove all listings and images
            db.Images.RemoveRange(db.Images);
            db.Listings.RemoveRange(db.Listings);
            db.SaveChanges();
        }
    }
    catch{// ignore errors during cleanup to avoid breaking startup
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Serve downloaded images from the Images folder
var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Images");
if (!Directory.Exists(imagesPath)) Directory.CreateDirectory(imagesPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/Images"
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();

app.Run();
