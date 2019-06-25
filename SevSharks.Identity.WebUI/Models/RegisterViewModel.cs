using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SevSharks.Identity.WebUI.Models
{
    /// <summary>
    /// Модель регистрации пользователя
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Email пользователя.
        /// </summary>
        [DataType(DataType.EmailAddress)]
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Login { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        /// <summary>
        /// Подтверждение пароля
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Телефон пользователя.
        /// </summary>
        [DataType(DataType.PhoneNumber)]
        [Required]
        [Display(Name = "Телефон")]
        public string Phone { get; set; }

        /// <summary>
        /// ReturnUrl
        /// </summary>
        [Required]
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Флаг, показывающий успешность авторизации
        /// </summary>
        public bool IsSucceed { get; set; }

        /// <summary>
        /// Сообщение об ошибках
        /// </summary>
        public List<string> ErrorMessages { get; set; }
    }
}
