using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SevSharks.Identity.DataAccess.Models
{
    public class ApplicationRole : IdentityRole
    {
        /// <summary>
        /// Navigation property for the users in this role.
        /// </summary>
        public virtual ICollection<IdentityUserRole<string>> Users { get; set; }

        /// <summary>
        /// Navigation property for claims in this role.
        /// </summary>
        public virtual ICollection<IdentityRoleClaim<string>> Claims { get; set; }
    }
}