namespace SevSharks.Identity.Contracts
{
    /// <summary>
    /// Информация по бонусному счету
    /// </summary>
    public class BonusAccountInfo
    {
        /// <summary>
        /// Номер бонусного счета
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Баланс бонусного счета
        /// </summary>
        public decimal Balance { get; set; }
    }
}
