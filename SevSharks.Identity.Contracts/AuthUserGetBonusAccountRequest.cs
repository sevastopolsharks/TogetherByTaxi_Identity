using SolarLab.Common.Contracts;

namespace SevSharks.Identity.Contracts
{
    /// <summary>
    /// Запрос на получение информации о бонусном счете
    /// </summary>
    public class AuthUserGetBonusAccountRequest : BaseGuidEvent, IWithQueueName, IWithCurrentUserId
    {
        /// <summary>
        /// Название очереди
        /// </summary>
        public string QueueName => TradingAuthQueues.TradingAuthQueuesGetBonusAccountInfoQueue;

        /// <summary>
        /// Идентификатор текущего пользователя
        /// </summary>
        public string CurrentUserId { get; set; }
    }
}
