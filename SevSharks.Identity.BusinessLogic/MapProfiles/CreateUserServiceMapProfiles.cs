using System;
using AutoMapper;
using SevSharks.Identity.BusinessLogic.Models;
using SevSharks.Identity.DataAccess.Models;

namespace SevSharks.Identity.BusinessLogic.MapProfiles
{
    public class CreateUserServiceMapProfiles : Profile
    {
        public CreateUserServiceMapProfiles()
        {
            CreateMap<UpdateUserDto, ApplicationUser>()
                .ForMember(d => d.IdFromIt1, map => map.MapFrom(s => s.ExternalId))
                .ForMember(x => x.Claims, y => y.Ignore())
                .ForMember(x => x.NormalizedUserName, y => y.Ignore())
                .ForMember(x => x.NormalizedEmail, y => y.Ignore())
                .ForMember(x => x.PasswordHash, y => y.Ignore())
                .ForMember(x => x.SecurityStamp, y => y.Ignore())
                .ForMember(x => x.ConcurrencyStamp, y => y.Ignore())
                .ForMember(x => x.TwoFactorEnabled, y => y.Ignore())
                .ForMember(x => x.LockoutEnd, y => y.Ignore())
                .ForMember(x => x.LockoutEnabled, y => y.Ignore())
                .ForMember(x => x.AccessFailedCount, y => y.Ignore())
                .ForMember(x => x.ExternalLogins, y => y.Ignore())
                .ForMember(x => x.Id, y => y.Ignore());

            CreateMap<CreateUserDto, ApplicationUser>()
                .ForMember(d => d.Id, map => map.MapFrom(s => Guid.NewGuid().ToString()))
                .ForMember(d => d.IdFromIt1, map => map.MapFrom(s => s.ExternalId))
                .ForMember(x => x.Claims, y => y.Ignore())
                .ForMember(x => x.NormalizedUserName, y => y.Ignore())
                .ForMember(x => x.NormalizedEmail, y => y.Ignore())
                .ForMember(x => x.PasswordHash, y => y.Ignore())
                .ForMember(x => x.SecurityStamp, y => y.Ignore())
                .ForMember(x => x.ConcurrencyStamp, y => y.Ignore())
                .ForMember(x => x.TwoFactorEnabled, y => y.Ignore())
                .ForMember(x => x.LockoutEnd, y => y.Ignore())
                .ForMember(x => x.LockoutEnabled, y => y.Ignore())
                .ForMember(x => x.ExternalLogins, y => y.Ignore())
                .ForMember(x => x.AccessFailedCount, y => y.Ignore());

            CreateMap<CreateUserDto, UpdateUserDto>();
        }
    }
}
