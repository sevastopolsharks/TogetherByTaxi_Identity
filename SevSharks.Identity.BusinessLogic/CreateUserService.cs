using IdentityModel;
using SevSharks.Identity.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using SevSharks.Identity.DataAccess;
using SevSharks.Identity.BusinessLogic.Models;

namespace SevSharks.Identity.BusinessLogic
{
    public class CreateUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Context _context;
        private readonly IMapper _mapper;

        public CreateUserService(UserManager<ApplicationUser> userManager, Context context, IMapper mapper)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApplicationUser> CreateUser(CreateUserDto userDto)
        {
            var user = await _userManager.FindByNameAsync(userDto.UserName);
            if (user != null)
            {
                var existingUser = _context.Users
                    .FirstOrDefault(u => u.Id == user.Id);

                return await UpdateUser(existingUser, _mapper.Map<CreateUserDto, UpdateUserDto>(userDto));
            }

            user = _mapper.Map<CreateUserDto, ApplicationUser>(userDto);

            if (userDto.ExternalSystemIdentifier != null && userDto.ExternalSystemName != null)
            {
                user.ExternalLogins = new List<UserExternalLogin>
                {
                    new UserExternalLogin
                    {
                        UserId = user.Id,
                        ExternalSystemName = userDto.ExternalSystemName,
                        ExternalUserName = userDto.ExternalSystemIdentifier
                    }
                };
            }

            IdentityResult result;
            //TODO: сделать создание пользователя с логином в латинице
            if (string.IsNullOrEmpty(userDto.Password))
            {
                result = await _userManager.CreateAsync(user);
            }
            else
            {
                result = await _userManager.CreateAsync(user, userDto.Password);
            }
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Name, userDto.UserName),
                new Claim(JwtClaimTypes.GivenName, userDto.UserName),
                new Claim(JwtClaimTypes.FamilyName, userDto.UserName)
            };

            result = await _userManager.AddClaimsAsync(user, claims);
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            return user;
        }

        public async Task<ApplicationUser> UpdateUser(ApplicationUser user, UpdateUserDto userDto)
        {
            _mapper.Map(userDto, user);
            if (!string.IsNullOrEmpty(userDto.Password))
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, userDto.Password);
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return user;
            }

            throw new Exception(result.Errors.First().Description);
        }
    }
}
