using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("api/data", async (
    [FromServices] IConfiguration configuration,
    HttpContext httpContext) =>
{
    try
    {
        var reader = new StreamReader(httpContext.Request.Body);
        var xml = await reader.ReadToEndAsync();

        var xmlData = XElement.Parse(xml);

        var dirPath = configuration["FilePath"];
        
        if (!Directory.Exists(dirPath))
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }

        var fileName = $"{Guid.NewGuid()}.xml";
        var filePath = Path.Combine(dirPath, fileName);

        xmlData.Save(filePath);

        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = "Invalid XML data", error = ex.Message });
    }
})
.Accepts<string>("application/xml")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status500InternalServerError)
.WithName("Data")
.WithOpenApi();

app.Run();

