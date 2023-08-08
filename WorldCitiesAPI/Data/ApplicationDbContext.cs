using Microsoft.EntityFrameworkCore;
using WorldCitiesAPI.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WorldCitiesAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() 
            : base () { }

        public ApplicationDbContext(DbContextOptions options)
            : base(options) { }

        public DbSet<City> Cities => Set<City> ();
        public DbSet<Country> Countries => Set<Country> ();
    }
}
