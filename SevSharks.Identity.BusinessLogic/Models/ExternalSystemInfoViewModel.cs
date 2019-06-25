using System.Collections.Generic;

namespace SevSharks.Identity.BusinessLogic.Models
{
    /// <summary>
    /// Вьюмодель для добавления существующему пользователю внешней системы
    /// </summary>
    public class ExternalSystemInfoViewModel
    {
        /// <summary>
        /// Список внешних систем, привязанных к пользователю
        /// </summary>
        public List<string> UserExternalSystemNames { get; set; } = new List<string>();

        /// <summary>
        /// Список внешних систем, не привязанных к пользователю
        /// </summary>
        public List<string> RemainExternalSystemNames { get; set; } = new List<string>();
    }
}
