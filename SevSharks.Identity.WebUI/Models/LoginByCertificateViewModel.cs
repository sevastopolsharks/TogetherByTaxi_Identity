namespace SevSharks.Identity.WebUI.Models
{
    /// <summary>
    /// Модель логина по сертификату
    /// </summary>
    public class LoginByCertificateViewModel
    {
        /// <summary>
        /// Данные подписанные сертификатом пользователя.
        /// </summary>
        public string SignedData { get; set; }

        /// <summary>
        /// URL возврата
        /// </summary>
        public string ReturnUrl { get; set; }
    }
}