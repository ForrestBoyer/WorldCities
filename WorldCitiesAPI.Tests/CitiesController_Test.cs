﻿using Microsoft.EntityFrameworkCore;
using WorldCitiesAPI.Controllers;
using WorldCitiesAPI.Data;
using WorldCitiesAPI.Data.Models;

namespace WorldCitiesAPI.Tests
{
    public class CitiesController_Test
    {
        [Fact]
        public async Task GetCity()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "WorldCities")
                .Options;
            using var context = new ApplicationDbContext(options);
            context.Add(new City()
            {
                Id = 1,
                CountryId = 1,
                Lat = 1,
                Lon = 1,
                Name = "TestCity1"
            });
            context.SaveChanges();

            var controller = new CitiesController(context);
            City? city_existing = null;
            City? city_nonExisting = null;

            // Act
            city_existing = (await controller.GetCity(1)).Value;
            city_nonExisting = (await controller.GetCity(2)).Value;

            // Assert
            Assert.NotNull(city_existing);
            Assert.Null(city_nonExisting);
        }
    }
}
