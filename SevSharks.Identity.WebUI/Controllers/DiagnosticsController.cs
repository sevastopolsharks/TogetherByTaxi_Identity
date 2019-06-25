using SevSharks.Identity.WebUI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using SevSharks.Identity.WebUI.Helpers;

namespace SevSharks.Identity.WebUI.Controllers
{
    /// <summary>
    /// DiagnosticsController
    /// </summary>
    [SecurityHeaders]
    [Authorize]
    public class DiagnosticsController : Controller
    {
        /// <summary>
        /// Index
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var localAddresses = new [] { "127.0.0.1", "::1", HttpContext.Connection.LocalIpAddress.ToString() };
            if (!localAddresses.Contains(HttpContext.Connection.RemoteIpAddress.ToString()))
            {
                return NotFound();
            }

            var model = new DiagnosticsViewModel(await HttpContext.AuthenticateAsync());
            return View(model);
        }
    }
}