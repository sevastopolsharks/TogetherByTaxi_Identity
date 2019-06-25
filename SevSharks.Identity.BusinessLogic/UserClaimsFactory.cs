using IdentityModel;
using SevSharks.Identity.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SevSharks.Identity.BusinessLogic
{
    public class UserClaimsFactory<TUser, TRole> : UserClaimsPrincipalFactory<TUser, TRole>
        where TUser : class
        where TRole : class
    {
        public UserClaimsFactory(UserManager<TUser> userManager, RoleManager<TRole> roleManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, roleManager, optionsAccessor)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(TUser user)
        {
            var principal = await base.CreateAsync(user);
            var identity = principal.Identities.First();

            var username = await UserManager.GetUserNameAsync(user);
            var usernameClaim = identity.FindFirst(claim => claim.Type == Options.ClaimsIdentity.UserNameClaimType && claim.Value == username);
            if (usernameClaim != null)
            {
                identity.RemoveClaim(usernameClaim);
                identity.AddClaim(new Claim(JwtClaimTypes.PreferredUserName, username));
            }

            if (!identity.HasClaim(x => x.Type == JwtClaimTypes.Name))
            {
                identity.AddClaim(new Claim(JwtClaimTypes.Name, username));
            }

            if (UserManager.SupportsUserEmail)
            {
                var email = await UserManager.GetEmailAsync(user);
                if (!string.IsNullOrWhiteSpace(email))
                {
                    identity.AddClaims(new[]
                    {
                        new Claim(JwtClaimTypes.Email, email),
                        new Claim(JwtClaimTypes.EmailVerified,
                            await UserManager.IsEmailConfirmedAsync(user) ? "true" : "false", ClaimValueTypes.Boolean)
                    });
                }
            }

            if (UserManager.SupportsUserPhoneNumber)
            {
                var phoneNumber = await UserManager.GetPhoneNumberAsync(user);
                if (!string.IsNullOrWhiteSpace(phoneNumber))
                {
                    identity.AddClaims(new[]
                    {
                        new Claim(JwtClaimTypes.PhoneNumber, phoneNumber),
                        new Claim(JwtClaimTypes.PhoneNumberVerified,
                            await UserManager.IsPhoneNumberConfirmedAsync(user) ? "true" : "false", ClaimValueTypes.Boolean)
                    });
                }
            }

            return principal;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TUser user)
        {
            var identity = await base.GenerateClaimsAsync(user).ConfigureAwait(false);
            Console.WriteLine(identity.AuthenticationType);

            if (!identity.HasClaim(x => x.Type == JwtClaimTypes.Subject))
            {
                var sub = (user as ApplicationUser)?.Id;
                identity.AddClaim(new Claim(JwtClaimTypes.Subject, sub));
            }

            if (!identity.HasClaim(x => x.Type == JwtClaimTypes.IdentityProvider))
            {
                identity.AddClaim(new Claim(JwtClaimTypes.IdentityProvider, IdentityServer4.IdentityServerConstants.LocalIdentityProvider));
            }

            if (!identity.HasClaim(x => x.Type == JwtClaimTypes.AuthenticationTime))
            {
                identity.AddClaim(new Claim(JwtClaimTypes.AuthenticationTime, ConvertToTimestamp(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));
            }

            return identity;
        }

        private long ConvertToTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Convert.ToInt64(Math.Floor(diff.TotalSeconds));
        }
    }
}

