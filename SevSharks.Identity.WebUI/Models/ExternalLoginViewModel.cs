using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SevSharks.Identity.WebUI.Models
{
    /// <summary>
    /// ExternalLoginViewModel
    /// </summary>
    public class ExternalLoginViewModel
    {
        /// <summary>
        /// Login
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Логин (email)")]
        public string Login { get; set; }

        /// <summary>
        /// Телефон пользователя.
        /// </summary>
        [DataType(DataType.PhoneNumber)]
        [Required]
        [Display(Name = "Телефон")]
        public string Phone { get; set; }

        /// <summary>
        /// Флаг, показывающий успешность авторизации
        /// </summary>
        public bool IsSucceed { get; set; }

        /// <summary>
        /// Сообщение об ошибках
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Идентификатор, возвращаемый внешней системой
        /// </summary>
        public string ExternalSystemIdentifier { get; set; }

        /// <summary>
        /// Имя внешней системы
        /// </summary>
        public string ExternalSystemName { get; set; }
    }
}
