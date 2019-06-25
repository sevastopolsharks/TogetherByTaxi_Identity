namespace SevSharks.Identity.BusinessLogic.Models
{
    public class CreateUserDto
    {
        public string ExternalId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string Password { get; set; }
        public byte[] PasswordHashFromIt1 { get; set; }
        public string RoleNameFromIt1 { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string ExternalSystemIdentifier { get; set; }
        public string ExternalSystemName { get; set; }
        public string BonusAccountNumber { get; set; }
        public decimal BonusAccountBalance { get; set; }
    }
}
