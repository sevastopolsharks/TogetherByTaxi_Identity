using AutoMapper;
using SevSharks.Identity.Contracts;
using SevSharks.Identity.DataAccess.Models;

namespace SevSharks.Identity.BusinessLogic.MapProfiles
{
    /// <summary>
    /// UserMapProfile
    /// </summary>
    public class UserMapProfile : Profile
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UserMapProfile()
        {
            CreateMap<ApplicationUser, UserInfo>()
                .ForMember(d => d.Id, map => map.MapFrom(s => s.Id))
                .ForMember(d => d.IdFromIt1, map => map.MapFrom(s => s.IdFromIt1))
                .ForMember(d => d.Name, map => map.MapFrom(s => s.UserName))
                .ForMember(d => d.Email, map => map.MapFrom(s => s.Email))
                .ForMember(d => d.EmailConfirmed, map => map.MapFrom(s => s.EmailConfirmed))
                .ForMember(d => d.Phone, map => map.MapFrom(s => s.PhoneNumber))
                .ForMember(d => d.PhoneConfirmed, map => map.MapFrom(s => s.PhoneNumberConfirmed))
                .ForMember(d => d.UserExternalLogins, map => map.MapFrom(s => s.ExternalLogins));

            CreateMap<UserExternalLogin, UserExternalLoginInfo>()
                .ForMember(d => d.ExternalUserName, map => map.MapFrom(s => s.ExternalUserName))
                .ForMember(d => d.ExternalSystemName, map => map.MapFrom(s => s.ExternalSystemName));
        }
    }
}
