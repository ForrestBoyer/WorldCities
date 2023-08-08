using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Threading.Tasks;
using WorldCitiesAPI.Controllers;
using WorldCitiesAPI.Data;
using WorldCitiesAPI.Data.Models;
using Xunit;

namespace WorldCitiesAPI.Tests
{
    public class SeedController_Tests
    {
        /// <summary>
        /// Test the CreateDefaultUsers() method
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateDefaultUsers()
        {
            // ARRANGE
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "WorldCities")
                .Options;

            var mockEnv = Mock.Of<IWebHostEnvironment>();

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(x => x[It.Is<String>(s => s == "DefaultPasswords:RegisteredUser")]).Returns("M0ckPa$$word");
            mockConfiguration.SetupGet(x => x[It.Is<String>(s => s == "DefaultPasswords:Administrator")]).Returns("M0ckPa$$word");

            using var context = new ApplicationDbContext(options);

            var roleManager = IdentityHelper.GetRoleManager(new RoleStore<IdentityRole>(context));

            var userManager = IdentityHelper.GetUserManager(new UserStore<ApplicationUser>(context));

            var controller = new SeedController(
                    context,
                    mockEnv,
                    roleManager,
                    userManager,
                    mockConfiguration.Object
                );

            ApplicationUser user_Admin = null!;
            ApplicationUser user_User = null!;
            ApplicationUser user_NotExisting = null!;

            // ACT
            await controller.CreateDefaultUsers();

            user_Admin = await userManager.FindByEmailAsync("admin@email.com");
            user_User = await userManager.FindByEmailAsync("user@email.com");
            user_NotExisting = await userManager.FindByEmailAsync("notexisting@email.com");

            // ASSERT
            Assert.NotNull(user_Admin);
            Assert.NotNull(user_User);
            Assert.Null(user_NotExisting);
        }
    }
}
