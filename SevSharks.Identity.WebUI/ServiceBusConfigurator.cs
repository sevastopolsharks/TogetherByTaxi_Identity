using SevSharks.Identity.Contracts;
using MassTransit;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using AutoMapper;
using SevSharks.Identity.BusinessLogic;
using SevSharks.Identity.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace SevSharks.Identity.WebUI
{
    /// <summary>
    /// ServiceBusConfigurator
    /// </summary>
    public static class ServiceBusConfigurator
    {
        /// <summary>
        /// Получить полную конфигурацию для шины
        /// </summary>
        public static Dictionary<string, Action<IRabbitMqReceiveEndpointConfigurator>> GetBusConfigurations(IServiceProvider serviceProvider)
        {
            var busConfig = new Dictionary<string, Action<IRabbitMqReceiveEndpointConfigurator>>
            {
                {
                    TradingAuthQueues.TradingAuthQueuesGetBonusAccountInfoQueue,
                    e => e.Handler<AuthUserGetBonusAccountRequest>(async ctx =>
                    {
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var logger = scope.ServiceProvider.GetService<ILogger>();
                            logger.LogDebug($"Пришло событие AuthUserGetBonusAccountRequest {ctx.Message.Id}");
                            try
                            {
                                var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                                var applicationUser = await userManager.FindByIdAsync(ctx.Message.CurrentUserId);

                                BonusAccountInfo bonusAccountInfo = null;
                                if (applicationUser != null)
                                {
                                    bonusAccountInfo = new BonusAccountInfo
                                    {
                                        Balance = applicationUser.BonusAccountBalance,
                                        Number = applicationUser.BonusAccountNumber
                                    };
                                }
                                var authEmployeeGetResponse = new AuthUserGetBonusAccountResponse
                                {
                                    IsSuccess = true,
                                    Result = bonusAccountInfo
                                };
                                await ctx.RespondAsync(authEmployeeGetResponse);
                            }
                            catch (Exception exception)
                            {
                                logger.LogError(exception, exception.Message);
                                var authEmployeeGetResponse = new AuthUserGetBonusAccountResponse
                                {
                                    IsSuccess = false,
                                    ErrorMessages = new List<string> {exception.Message},
                                    Result = null
                                };
                                await ctx.RespondAsync(authEmployeeGetResponse);
                            }
                        }
                    })
                },
                {
                    TradingAuthQueues.TradingAuthQueuesGetUserInfoQueue,
                    e => e.Handler<AuthUserGetInfoRequest>(async ctx =>
                    {
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var logger = scope.ServiceProvider.GetService<ILogger>();
                            logger.LogDebug($"Пришло событие AuthUserGetInfoRequest {ctx.Message.CurrentUserId}");
                            try
                            {
                                var externalSystemAccountService = scope.ServiceProvider.GetService<ExternalSystemAccountService>();
                                var user = await externalSystemAccountService.GetUsersWithExternalSystemAccounts(ctx.Message.CurrentUserId);
                                UserInfo userInfo = null;
                                if (user != null)
                                {
                                    var mapper = scope.ServiceProvider.GetService<IMapper>();
                                    userInfo = mapper.Map<ApplicationUser, UserInfo>(user);
                                }

                                var authUserGetInfoResponse = new AuthUserGetInfoResponse
                                {
                                    IsSuccess = true,
                                    Result = userInfo
                                };
                                await ctx.RespondAsync(authUserGetInfoResponse);
                            }
                            catch (Exception exception)
                            {
                                logger.LogError(exception, exception.Message);
                                var authUserGetInfoResponse = new AuthUserGetInfoResponse
                                {
                                    IsSuccess = false,
                                    ErrorMessages = new List<string> {exception.Message},
                                    Result = null
                                };
                                await ctx.RespondAsync(authUserGetInfoResponse);
                            }
                        }
                    })
                }
            };
            return busConfig;
        }
    }
}
