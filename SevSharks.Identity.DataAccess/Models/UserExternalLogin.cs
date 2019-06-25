using System;

namespace SevSharks.Identity.DataAccess.Models
{
    /// <summary>
    /// Данные о регистрации пользователя во внешней системе
    /// </summary>
    public class UserExternalLogin : BaseEntity<Guid>
    {
        /// <summary>
        /// Имя пользователя во внешней системе (Идентификатор возвращаемый системой)
        /// </summary>
        public string ExternalUserName { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// Имя внешней системы
        /// </summary>
        public string ExternalSystemName{ get; set; }
    }
}
