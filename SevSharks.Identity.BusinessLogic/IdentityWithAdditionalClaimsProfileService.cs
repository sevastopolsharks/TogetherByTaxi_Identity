using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using SevSharks.Identity.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SevSharks.Identity.BusinessLogic
{
    public class IdentityWithAdditionalClaimsProfileService : IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityWithAdditionalClaimsProfileService(UserManager<ApplicationUser> userManager,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.GetSubjectId();
            var user = await _userManager.Users
                .SingleOrDefaultAsync(x => x.Id == subjectId);

            var principal = await _claimsFactory.CreateAsync(user);

            var claims = principal.Claims.ToList();

            if (context.RequestedClaimTypes != null && context.RequestedClaimTypes.Any())
            {
                claims = claims.Where(x => context.RequestedClaimTypes.Where(r => r != CustomJwtClaimTypes.Permission && r != JwtClaimTypes.Role).Contains(x.Type)).ToList();

                if (context.RequestedClaimTypes.Any(x => x == CustomJwtClaimTypes.Permission))
                {
                    //TODO: add correct permissions
                    /*
                    claims.AddRange(new[]
                    {
                        new Claim(CustomJwtClaimTypes.EmployeeIdentifier, $"{response.Result.Id}")
                    });

                    var allAvailablePermissionsForOrganization = new List<string>();
                    foreach (var organizationRole in response.Result.Organization.OrganizationRoles)
                    {
                        if (organizationRole.BusinessPermissions == null)
                        {
                            continue;
                        }

                        foreach (var businessPermission in organizationRole.BusinessPermissions)
                        {
                            allAvailablePermissionsForOrganization.Add(businessPermission.Name.ToLower());
                        }
                    }

                    foreach (var employeeRoleInfo in response.Result.EmployeeRoles)
                    {
                        if (employeeRoleInfo.BusinessPermissions == null ||
                            !employeeRoleInfo.BusinessPermissions.Any())
                        {
                            continue;
                        }

                        foreach (var businessPermissionInfo in employeeRoleInfo.BusinessPermissions)
                        {
                            if (allAvailablePermissionsForOrganization.Contains(
                                businessPermissionInfo.Name.ToLower()))
                            {
                                claims.Add(new Claim(CustomJwtClaimTypes.Permission, businessPermissionInfo.Name));
                            }
                        }
                    }
                    */
                }

                claims.AddRange(new[]
                {
                    new Claim(JwtClaimTypes.Name, $"{user.UserName}")
                });

                if (_userManager.SupportsUserRole)
                {
                    var roleClaims =
                        from role in await _userManager.GetRolesAsync(user)
                        select new Claim(JwtClaimTypes.Role, role);
                    claims.AddRange(roleClaims);
                }

                if (_userManager.SupportsUserEmail)
                {
                    var email = await _userManager.GetEmailAsync(user);
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        claims.AddRange(new[]
                        {
                            new Claim(JwtClaimTypes.Email, email),
                            new Claim(JwtClaimTypes.EmailVerified,
                                await _userManager.IsEmailConfirmedAsync(user) ? "true" : "false",
                                ClaimValueTypes.Boolean)
                        });
                    }
                }

                if (_userManager.SupportsUserPhoneNumber)
                {
                    var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
                    if (!string.IsNullOrWhiteSpace(phoneNumber))
                    {
                        claims.AddRange(new[]
                        {
                            new Claim(JwtClaimTypes.PhoneNumber, phoneNumber),
                            new Claim(JwtClaimTypes.PhoneNumberVerified,
                                await _userManager.IsPhoneNumberConfirmedAsync(user) ? "true" : "false",
                                ClaimValueTypes.Boolean)
                        });
                    }
                }

                context.IssuedClaims = claims;
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}