using SolarLab.Common.Contracts;

namespace SevSharks.Identity.Contracts
{
    public class AuthUserGetInfoRequest : IWithQueueName, IWithCurrentUserId
    {
        /// <summary>
        /// Название очереди
        /// </summary>
        public string QueueName => TradingAuthQueues.TradingAuthQueuesGetUserInfoQueue;

        /// <summary>
        /// Идентификатор текущего пользователя
        /// </summary>
        public string CurrentUserId { get; set; }
    }
}
