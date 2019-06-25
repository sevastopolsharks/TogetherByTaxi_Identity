using SevSharks.Identity.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SevSharks.Identity.BusinessLogic
{
    public class ExternalSystemAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ExternalSystemAccountService> _logger;

        public ExternalSystemAccountService(UserManager<ApplicationUser> userManager,
            ILogger<ExternalSystemAccountService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<List<UserExternalLogin>> GetUsersExternalSystemAccounts(string userId)
        {
            var user = await GetUserWithExternalLoginsById(userId);
            return user.ExternalLogins;
        }

        public async Task<ApplicationUser> GetUsersWithExternalSystemAccounts(string userId)
        {
            var user = await GetUserWithExternalLoginsById(userId);
            return user;
        }

        public async Task<ApplicationUser> AddUsersExternalSystemAccount(string userId, string loginProvider, string providerKey)
        {
            var user = await GetUserWithExternalLoginsById(userId);

            if (user.ExternalLogins == null)
            {
                user.ExternalLogins = new List<UserExternalLogin>();
            }

            user.ExternalLogins.Add(new UserExternalLogin
            {
                UserId = userId,
                ExternalSystemName = loginProvider,
                ExternalUserName = providerKey
            });

            try
            {
                await _userManager.UpdateAsync(user);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw;
            }
            return user;
        }

        public async Task<ApplicationUser> DeleteUsersExternalSystemAccount(string userId, string loginProvider)
        {
            var user = await GetUserWithExternalLoginsById(userId);
            if (user.ExternalLogins == null || !user.ExternalLogins.Any())
            {
                _logger.LogError($"У пользователя {userId} не найдено внешних систем. Удаление системы {loginProvider} не возможно.");
                return user;
            }

            user.ExternalLogins.Remove(user.ExternalLogins.SingleOrDefault(
                                           s => s.UserId.Equals(userId) &&
                                                s.ExternalSystemName.Equals(loginProvider)));

            try
            {
                await _userManager.UpdateAsync(user);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw;
            }

            return user;
        }

        private async Task<ApplicationUser> GetUserWithExternalLoginsById(string userId)
        {
            return await _userManager.Users.Include(s => s.ExternalLogins).SingleOrDefaultAsync(s => s.Id.Equals(userId));
        }
    }
}
