using IdentityServer4.Services;
using IdentityServer4.Validation;
using SevSharks.Identity.BusinessLogic;
using SevSharks.Identity.DataAccess;
using SevSharks.Identity.DataAccess.Models;
using SevSharks.Identity.WebUI.esiaconnection;
using SevSharks.Identity.WebUI.okconnection;
using SevSharks.Identity.WebUI.vkconnection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolarLab.BusManager.Abstraction;
using SolarLab.BusManager.Implementation;
using System;
using System.Security.Claims;
using AutoMapper;
using SevSharks.Identity.BusinessLogic.MapProfiles;

namespace SevSharks.Identity.WebUI
{
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// IConfiguration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// ConfigureServices
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            void DbContextOptionsBuilder(DbContextOptionsBuilder builder)
            {
                string connectionString = GetSettingFromConfig(Configuration, "ConnectionStrings", "DefaultConnection");

                builder.UseNpgsql(connectionString,
                    b => b.MigrationsAssembly("SevSharks.Identity.DataAccess"));
            }

            services.AddDbContext<Context>(DbContextOptionsBuilder);
            services.AddIdentityServer()
                // TODO add certificate 
                //.AddSigningCredential(new X509Certificate2(Path.Combine(".", "Data\\Certificates", "IDecideAuth.pfx"),"", X509KeyStorageFlags.MachineKeySet))
                .AddDeveloperSigningCredential()
                // this adds the config data from DB (clients, resources)
                .AddConfigurationStore(options => options.ConfigureDbContext = DbContextOptionsBuilder)
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = DbContextOptionsBuilder;

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30;
                });
            services.AddSingleton(Configuration);
            services.AddSingleton((IConfigurationRoot)Configuration);
            services
                .AddIdentity<ApplicationUser, ApplicationRole>(opt =>
                {
                    // Basic built in validations
                    opt.Password.RequireDigit = false;
                    opt.Password.RequireLowercase = false;
                    opt.Password.RequireNonAlphanumeric = false;
                    opt.Password.RequireUppercase = false;
                    opt.Password.RequiredLength = 6;
                    opt.User.AllowedUserNameCharacters = null;
                    opt.User.RequireUniqueEmail = false;
                })
                .AddEntityFrameworkStores<Context>()
                .AddDefaultTokenProviders();
            var esiaClientId = GetSettingFromConfig(Configuration, "Esia", "ClientId");
            var esiaInformationSystemThumbprint = GetSettingFromConfig(Configuration, "Esia", "InformationSystemThumbprint");
            var esiaMainUrl = GetSettingFromConfig(Configuration, "Esia", "MainUrl");
            var esiaAuthorizationEndpointPostfix = GetSettingFromConfig(Configuration, "Esia", "AuthorizationEndpointPostfix");
            var esiaTokenEndpointPostfix = GetSettingFromConfig(Configuration, "Esia", "TokenEndpointPostfix");
            var esiaUserInformationEndpointPostfix = GetSettingFromConfig(Configuration, "Esia", "UserInformationEndpointPostfix");
            services
                .AddAuthentication()
                .AddGoogle("Google", options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.ClientId = "1042223585808-6vn7hjihdj189rhhpfp7sd4ps232lf83.apps.googleusercontent.com";
                    options.ClientSecret = "0YsEyRveKcUExq7km84LdKfX";
                })
                .AddFacebook("facebook", options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.ClientId = "1939113896168784";
                    options.AppSecret = "6b7fe1719ace991ef20c2ae01ebd4153";
                })
                .AddMicrosoftAccount(options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.ClientId = "0000000044240490";
                    options.ClientSecret = "zaBN451~tqzbcMRAPK36^@|";
                })
                .AddVk("ВКонтакте", options =>
                {
                    options.ApiVersion = "8.57";
                    options.ClientId = "6753829";
                    options.ClientSecret = "eRO6bhdQVhyy6Y00rE3O";
                    options.SignInScheme = IdentityConstants.ExternalScheme;

                    // Request for permissions https://vk.com/dev/permissions?f=1.%20Access%20Permissions%20for%20User%20Token
                    options.Scope.Add("email");

                    // Add fields https://vk.com/dev/objects/user
                    options.Fields.Add("uid");
                    options.Fields.Add("first_name");
                    options.Fields.Add("last_name");

                    // In this case email will return in OAuthTokenResponse, 
                    // but all scope values will be merged with user response
                    // so we can claim it as field
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "uid");
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "first_name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "last_name");
                })
                .AddOK("Одноклассники", options =>
                {
                    options.ClientId = "1273085952";
                    options.PublicApplicationKey = "CBABMCOMEBABABABA";
                    options.ClientSecret = "A751011563E78F41740122F9";
                    options.SignInScheme = IdentityConstants.ExternalScheme;

                    // Request for permissions email to Odnoklassniki API team
                    options.Scope.Add("VALUABLE_ACCESS");
                    options.Scope.Add("email");

                    // Add fields https://apiok.ru/dev/methods/rest/users/users.getCurrentUser
                    options.Fields.Add("uid");
                    options.Fields.Add("first_name");
                    options.Fields.Add("last_name");
                    options.Fields.Add("email");

                    // In this case email will return in OAuthTokenResponse, 
                    // but all scope values will be merged with user response
                    // so we can claim it as field
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "uid");
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "first_name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "last_name");
                })
                .AddEsia("ЕСИА", options =>
                {
                    options.ClientId = esiaClientId;
                    options.InformationSystemThumbprint = esiaInformationSystemThumbprint;
                    options.MainUrl = esiaMainUrl;
                    options.AuthorizationEndpointPostfix = esiaAuthorizationEndpointPostfix;
                    options.TokenEndpointPostfix = esiaTokenEndpointPostfix;
                    options.UserInformationEndpointPostfix = esiaUserInformationEndpointPostfix;
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                });
            services.AddTransient<UserManager<ApplicationUser>>();
            services.AddTransient<ExternalSystemAccountService>();
            //services.AddSingleton<ILog, Log>();
            services.AddSingleton<IBusManager, MassTransitBusManager>();
            services.AddTransient<IProfileService, IdentityWithAdditionalClaimsProfileService>();
            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
            services.AddTransient<IUserClaimsPrincipalFactory<ApplicationUser>,
                    UserClaimsFactory<ApplicationUser, ApplicationRole>>();
            services.AddTransient<CreateUserService, CreateUserService>();
            services.AddSingleton<IMapper>(new Mapper(GetMapperConfiguration()));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                //app.UseHsts();//TODO: add when we need https
            }

            //app.UseHttpsRedirection();//TODO: add when we need https
            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }

        private string GetSettingFromConfig(IConfiguration configuration, string firstName, string secondName)
        {
            //var firstName = "ConnectionStrings";
            //var secondName = "DefaultConnection";
            string result = configuration[firstName + ":" + secondName];
            if (string.IsNullOrEmpty(result))
            {
                result = configuration[firstName + "_" + secondName];
            }

            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("Configuration setting does not exist. Setting name " + firstName + ":" + secondName);
            }

            return result;
        }

        private static MapperConfiguration GetMapperConfiguration()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CreateUserServiceMapProfiles>();
                cfg.AddProfile<UserMapProfile>();
            });

            configuration.AssertConfigurationIsValid();

            return configuration;
        }
    }
}