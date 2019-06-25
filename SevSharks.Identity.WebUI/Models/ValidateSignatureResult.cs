namespace SevSharks.Identity.WebUI.Models
{
    /// <summary>
    /// Модель возвращаемого значения метода валидации подписи
    /// </summary>
    public class ValidateSignatureResult
    {
        /// <summary>
        /// Ошибка подписи
        /// </summary>
        public string SignError { get; set; }
    }
}