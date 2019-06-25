namespace SevSharks.Identity.Contracts
{
    /// <summary>
    /// Данные о регистрации пользователя во внешней системе
    /// </summary>
    public class UserExternalLoginInfo
    {
        /// <summary>
        /// Имя пользователя во внешней системе (Идентификатор возвращаемый системой)
        /// </summary>
        public string ExternalUserName { get; set; }

        /// <summary>
        /// Имя внешней системы
        /// </summary>
        public string ExternalSystemName { get; set; }
    }
}
