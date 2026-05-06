using Assignment_4.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Assignment_4.Data;

public class Dat154Gr2ContextFactory : IDesignTimeDbContextFactory<Dat154Gr2Context>
{
    public Dat154Gr2Context CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<Dat154Gr2Context>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("Dat154Connection"));
        return new Dat154Gr2Context(optionsBuilder.Options);
    }
}
