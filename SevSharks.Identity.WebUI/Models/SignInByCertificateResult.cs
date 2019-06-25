namespace SevSharks.Identity.WebUI.Models
{
    /// <summary>
    /// Модель возвращаемого значения метода логина по сертификату
    /// </summary>
    public class SignInByCertificateResult
    {
        /// <summary>
        /// Ошибка, возникшая в ходе выполнения метода
        /// </summary>
        public string Error { get; set; }
    }
}