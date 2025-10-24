using Microsoft.Extensions.FileProviders;
using MyFirstApp.CustomConstraints;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    WebRootPath = "images"
});


builder.Services.AddRouting(options =>
{
    options.ConstraintMap.Add("months", typeof(MonthsCustomConstraint));
});
var app = builder.Build();

app.UseStaticFiles(); //works with 1st WebRootPath
//app.UseStaticFiles(new StaticFileOptions()
//{
//    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath + @"\images2"))
//});

app.Map("files/{filename}.{extension}", async (context) =>
{
    string? fileName = Convert.ToString(context.Request.RouteValues["filename"]);
    string? extension = Convert.ToString(context.Request.RouteValues["extension"]);
    await context.Response.WriteAsync($"In file: {fileName} - {extension}");
});

app.Map("employee/profile/{employeename:length(4,7):alpha=john}", async (context) =>
{
    string? employeeName = Convert.ToString(context.Request.RouteValues["employeename"]);
    await context.Response.WriteAsync($"Employee: {employeeName}");
});

app.Map("/product/details/{id:int:range(1,1000)?}", async (context) =>
{
    if (context.Request.RouteValues.ContainsKey("id"))
    {
        int id = Convert.ToInt32(context.Request.RouteValues["id"]);
        await context.Response.WriteAsync($"Product details of: {id}");
    }
    else
    {
        int id = Convert.ToInt32(context.Request.RouteValues["id"]);
        await context.Response.WriteAsync("Product Id is not supplied.");
    }

});

app.Map("/cities/{cityId:guid}", async (context) =>
{
    Guid id = Guid.Parse(Convert.ToString(context.Request.RouteValues["cityId"])!);

    await context.Response.WriteAsync($"City id: {id}");
});

//Daily report
app.Map("/daily-report/{reportdate:datetime}", async (context) => {
    DateTime reportDate = Convert.ToDateTime(context.Request.RouteValues["reportdate"]);
    await context.Response.WriteAsync($"In daily-report: {reportDate.ToShortDateString()}");
});


app.Map("/sales-report/{year:int:min(1900)}/{month:months}", async context =>
{
    int year = Convert.ToInt32(context.Request.RouteValues["year"]);
    string? month = Convert.ToString(context.Request.RouteValues["month"]);

    if(month=="jan" || month == "jul" || month == "oct"|| month == "apr")
    await context.Response.WriteAsync($"sales-report: {year}/{month}");
    else await context.Response.WriteAsync("This month is not allowed for sales report.");
});

app.Map("sales-report/2024/jan", async(context) =>
{
await context.Response.WriteAsync($"2025/January");
});

app.MapFallback(async (context) => {
    await context.Response.WriteAsync($"No route matched at: {context.Request.Path}");
});

app.Run();

