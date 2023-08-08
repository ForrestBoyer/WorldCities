using System.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCitiesAPI.Data;
using WorldCitiesAPI.Data.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;

namespace WorldCitiesAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public SeedController(ApplicationDbContext context, IWebHostEnvironment environment, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _env = environment;
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
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

        [HttpGet]
        public async Task<ActionResult> CreateDefaultUsers()
        {
            string role_RegisteredUser = "RegisteredUser";
            string role_Administrator = "Administrator";

            if (await _roleManager.FindByNameAsync(role_RegisteredUser) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(role_RegisteredUser));
            }

            if (await _roleManager.FindByNameAsync(role_Administrator) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(role_Administrator));
            }

            var addedUserList = new List<ApplicationUser>();

            var email_Admin = "admin@email.com";

            if (await _userManager.FindByNameAsync(email_Admin) == null)
            {
                var user_Admin = new ApplicationUser()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = email_Admin,
                    Email = email_Admin
                };

                await _userManager.CreateAsync(user_Admin, _configuration["DefaultPasswords:Administrator"]);

                await _userManager.AddToRoleAsync(user_Admin, role_RegisteredUser);
                await _userManager.AddToRoleAsync(user_Admin, role_Administrator);

                user_Admin.EmailConfirmed = true;
                user_Admin.LockoutEnabled = false;

                addedUserList.Add(user_Admin);
            }

            var email_User = "user@email.com";

            if (await _userManager.FindByNameAsync(email_User) == null)
            {
                var user_User = new ApplicationUser()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = email_User,
                    Email = email_User
                };

                await _userManager.CreateAsync(user_User, _configuration["DefaultPasswords:RegisteredUser"]);

                await _userManager.AddToRoleAsync(user_User, role_RegisteredUser);

                user_User.EmailConfirmed = true;
                user_User.LockoutEnabled = false;

                addedUserList.Add(user_User);
            }

            if (addedUserList.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return new JsonResult(new
            {
                Count = addedUserList.Count,
                Users = addedUserList
            });
        }
    }
}
