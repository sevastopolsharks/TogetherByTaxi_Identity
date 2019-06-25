using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using SevSharks.Identity.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace SevSharks.Identity.BusinessLogic
{
    public class ResourceOwnerPasswordValidator: IResourceOwnerPasswordValidator
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ResourceOwnerPasswordValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
 
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = await _userManager.FindByNameAsync(context.UserName);
            if (user != null)
            {
                if (await _userManager.CheckPasswordAsync(user, context.Password))
                {
                    context.Result = new GrantValidationResult(user.Id, OidcConstants.AuthenticationMethods.Password);
                    return;
                }
            }

            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Пользователь не найден");
        }
    }
}