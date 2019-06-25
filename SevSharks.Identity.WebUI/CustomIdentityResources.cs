using IdentityModel;
using IdentityServer4.Models;
using SevSharks.Identity.BusinessLogic;

namespace SevSharks.Identity.WebUI
{
    /// <summary>
    /// 
    /// </summary>
    public static class CustomIdentityResources
    {
        /// <summary>
        /// 
        /// </summary>
        public static class CustomScopes
        {
            public const string Roles = "roles";
            public const string Permissions = "permissions";
        }

        /// <summary>
        /// 
        /// </summary>
        public class Roles : IdentityResource
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public Roles()
            {
                Name = CustomScopes.Roles;
                DisplayName = "Ваши роли";
                Required = true;
                UserClaims = new [] { JwtClaimTypes.Role };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Permissions : IdentityResource
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public Permissions()
            {
                Name = CustomScopes.Permissions;
                DisplayName = "Ваши права доступа";
                Required = true;
                UserClaims = new [] { CustomJwtClaimTypes.Permission };
            }
        }
    }
}
