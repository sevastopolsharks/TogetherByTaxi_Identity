using System.ComponentModel.DataAnnotations;

namespace SevSharks.Identity.WebUI.Models
{
    /// <summary>
    /// LoginWithRecoveryCodeViewModel
    /// </summary>
    public class LoginWithRecoveryCodeViewModel
    {
        /// <summary>
        /// RecoveryCode
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; }
    }
}
