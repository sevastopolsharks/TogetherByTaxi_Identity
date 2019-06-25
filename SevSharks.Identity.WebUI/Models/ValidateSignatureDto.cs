namespace SevSharks.Identity.WebUI.Models
{
    /// <summary>
    /// ДТО валидации сертификата
    /// </summary>
    public class ValidateSignatureDto
    {
        /// <summary>
        /// Подпись
        /// </summary>
        public byte[] Signature { get; set; }
        
        /// <summary>
        /// Данные для подписи
        /// </summary>
        public byte[] BytesToSign { get; set; }

        /// <summary>
        /// Является ли открепленной
        /// </summary>
        public bool IsDetached { get; set; }
    }
}