namespace SevSharks.Identity.Contracts
{
    /// <summary>
    /// Название очередей для шины обмена данных
    /// </summary>
    public static class TradingAuthQueues
    {
        /// <summary>
        /// IT2.Trading.Auth.GetBonusAccountInfo
        /// </summary>
        public static string TradingAuthQueuesGetBonusAccountInfoQueue => "IT2.Trading.Auth.GetBonusAccountInfo";

        /// <summary>
        /// IT2.Trading.Auth.GetUserInfo
        /// </summary>
        public static string TradingAuthQueuesGetUserInfoQueue => "IT2.Trading.Auth.GetUserInfo";
    }
}
