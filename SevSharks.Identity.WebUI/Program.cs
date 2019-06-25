using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SolarLab.BusManager.Abstraction;
using System;
using Microsoft.Extensions.Logging;

namespace SevSharks.Identity.WebUI
{
    /// <summary>
    /// Основной старт выполнения
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Основной старт выполнения
        /// </summary>
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args);

            var services = host.Services;
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Auth started");
            try
            {
                var busManager = services.GetService<IBusManager>();
                busManager.StartBus(ServiceBusConfigurator.GetBusConfigurations(services));
            }
            catch (Exception ex)
            {
                logger.LogError("Error during start bus for Auth", ex);
            }

            // Seed data
            SeedData.EnsureSeedData(services);

            host.Run();
        }

        /// <summary>
        /// CreateWebHostBuilder
        /// </summary>
        public static IWebHost CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                        config.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);
                        config.AddJsonFile("secrets/appsettings.secrets.json", optional: true);
                        config.AddEnvironmentVariables();
                        config.AddCommandLine(args);
                    })
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                        logging.AddDebug();
                        logging.AddEventSourceLogger();
                    })
                   .UseSerilog()
                   .UseStartup<Startup>()
                   .Build();
    }
}
