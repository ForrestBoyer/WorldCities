using System.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML;
using WorldCitiesAPI.Data;
using WorldCitiesAPI.Data.Models;
using ClosedXML.Excel;

namespace WorldCitiesAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SeedController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _env = environment;
        }

        [HttpGet]
        public async Task<ActionResult> Import()
        {
            if (!_env.IsDevelopment())
            {
                throw new SecurityException("Not allowed");
            }

            var path = Path.Combine(_env.ContentRootPath, "Data/Source/worldcities.xlsx");

            var workbook = new XLWorkbook(path);
            var worksheet = workbook.Worksheet(1);
            var range = worksheet.RangeUsed();

            var existingCountries = await _context.Countries.ToListAsync();
            var existingCities = await _context.Cities.ToListAsync();

            int numberOfCountriesAdded = 0;
            int numberOfCitiesAdded = 0;

            foreach (var row in range.RowsUsed().Skip(1))
            {
                var country = new Country()
                {
                    Name = row.Cell(5).GetValue<string>(),
                    ISO2 = row.Cell(6).GetValue<string>(),
                    ISO3 = row.Cell(7).GetValue<string>()
                };

                if (existingCountries.Any(c => c.Name == country.Name))
                {
                    continue;
                }
                else
                {
                    await _context.Countries.AddAsync(country);
                    existingCountries.Add(country);
                    numberOfCountriesAdded++;
                }
            }

            if (numberOfCountriesAdded > 0)
            {
                await _context.SaveChangesAsync();
            }

            foreach (var row in range.RowsUsed().Skip(1))
            {
                var city = new City()
                {
                    Name = row.Cell(1).GetValue<string>(),
                    Lat = row.Cell(3).GetValue<decimal>(),
                    Lon = row.Cell(4).GetValue<decimal>(),
                    CountryId = existingCountries.First(c => c.Name == row.Cell(5).GetValue<string>()).Id
                };

                if (existingCities.Any(c => c.Name == city.Name && c.Lat == city.Lat && c.Lon == city.Lon))
                {
                    continue;
                }
                else
                {
                    await _context.Cities.AddAsync(city);
                    existingCities.Add(city);
                    numberOfCitiesAdded++;
                }
            }

            if (numberOfCitiesAdded > 0)
            {
                await _context.SaveChangesAsync();
            }

            return new JsonResult(new
            {
                Cities = numberOfCitiesAdded,
                Countries = numberOfCountriesAdded
            });
        }
    }
}
