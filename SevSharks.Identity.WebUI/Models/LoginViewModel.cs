using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SevSharks.Identity.WebUI.Models
{
    /// <summary>
    /// LoginViewModel
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Логин пользователя.
        /// </summary>
        [Display(Name = "Логин (email)")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Поле \"Логин\" обязательно к заполнению")]
        public string Login { get; set; }

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Поле \"Пароль\" обязательно к заполнению")]
        public string Password { get; set; }

        /// <summary>
        /// Данные подписанные сертификатом пользователя.
        /// </summary>
        public string SignedData { get; set; }

        /// <summary>
        /// Флаг, показывающий успешность авторизации
        /// </summary>
        public bool IsSucceed { get; set; }

        /// <summary>
        /// Сообщение об ошибке, при неудачном логине по серту или логину паролю
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Признак того, что была осуществлена попытка входа, используя сертификат
        /// </summary>
        public bool IsLogginedUsedCert { get; set; }

        /// <summary>
        /// AllowRememberLogin
        /// </summary>
        public bool AllowRememberLogin { get; set; }

        /// <summary>
        /// ReturnUrl
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// EnableLocalLogin
        /// </summary>
        public bool EnableLocalLogin { get; set; } = true;

        /// <summary>
        /// ExternalProviders
        /// </summary>
        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }

        /// <summary>
        /// VisibleExternalProviders
        /// </summary>
        public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders?.Where(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        /// <summary>
        /// IsExternalLoginOnly
        /// </summary>
        public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;

        /// <summary>
        /// ExternalLoginScheme
        /// </summary>
        public string ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;

        /// <summary>
        /// RedirectToRegister
        /// </summary>
        public bool RedirectToRegister { get; set; }

        /// <summary>
        /// Идентификатор клиента.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Параметр, указывающий допускается ли регистрация новых пользователей на ИТ2
        /// </summary>
        public bool AllowRegister { get; set; }
    }
}