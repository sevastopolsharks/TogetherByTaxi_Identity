using System;
using System.IO;
using SevSharks.Identity.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SevSharks.Identity.WebUI.Helpers
{
    /// <summary>
    /// Фабрика используется для миграций на postgress
    ///TODO: Исследовать, как будут накатываться миграции, возможно фабрику удалить 
    ///TODO: Исследовать, можно ли получить конфигурацию в конструкторе - из контрейнера
    /// </summary>
    public class DesignTimeContextFactory : IDesignTimeDbContextFactory<Context>
    {
        /// <summary>
        /// CreateDbContext
        /// </summary>
        public Context CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var builder = new DbContextOptionsBuilder<Context>();
            var connectionString = configuration["ConnectionStrings:DefaultConnection"];
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = configuration["ConnectionStrings_DefaultConnection"];
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Configuration incorrect: ConnectionStrings:DefaultConnection");
            }
            builder.UseNpgsql(connectionString, b => b.MigrationsAssembly("SevSharks.Identity.DataAccess"));
            return new Context(builder.Options);
        }
    }
}
