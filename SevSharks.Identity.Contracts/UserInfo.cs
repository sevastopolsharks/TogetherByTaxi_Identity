using System.Collections.Generic;

namespace SevSharks.Identity.Contracts
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string IdFromIt1 { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string Phone { get; set; }
        public bool PhoneConfirmed { get; set; }
        public List<UserExternalLoginInfo> UserExternalLogins { get; set; }
    }
}
