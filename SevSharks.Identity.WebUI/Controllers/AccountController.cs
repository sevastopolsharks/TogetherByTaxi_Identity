using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using SevSharks.Identity.BusinessLogic;
using SevSharks.Identity.BusinessLogic.Models;
using SevSharks.Identity.DataAccess.Models;
using SevSharks.Identity.WebUI.Helpers;
using SevSharks.Identity.WebUI.Helpers.Captcha;
using SevSharks.Identity.WebUI.Models;
using SevSharks.Identity.WebUI.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using IActionResult = Microsoft.AspNetCore.Mvc.IActionResult;

namespace SevSharks.Identity.WebUI.Controllers
{
    /// <summary>
    /// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    /// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
    /// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
    /// </summary>
    [SecurityHeaders]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly IConfiguration _configuration;
        private readonly CreateUserService _createUserService;
        private readonly ExternalSystemAccountService _externalSystemAccountService;
        private readonly ILogger<AccountController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public AccountController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IEventService events,
            CreateUserService createUserService,
            IConfiguration configuration,
            ExternalSystemAccountService externalSystemAccountService,
            ILogger<AccountController> logger)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _signInManager = signInManager;
            _userManager = userManager;
            _createUserService = createUserService;
            _configuration = configuration;
            _externalSystemAccountService = externalSystemAccountService;
            _logger = logger;
        }

        /// <summary>
        /// Show login page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var vm = await BuildLoginViewModelAsync(returnUrl);
            if (vm.RedirectToRegister)
            {
                TempData["ReturnUrl"] = returnUrl;
                return RedirectToAction("Register");
            }

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return ExternalLogin(vm.ExternalLoginScheme, returnUrl);
            }
            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                //ApplicationUser user = await _userManager.FindByNameAsync(model.Login);  
                var result = await _signInManager.PasswordSignInAsync(model.Login, model.Password, model.AllowRememberLogin, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return await LoginInner();
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Login, "invalid credentials"));
                ModelState.AddModelError("", AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);

            async Task<IActionResult> LoginInner()
            {
                var user = await _userManager.FindByNameAsync(model.Login);
                ProcessAuthenticationProperties(model);
                return await SignInAndRedirect(user, model.ReturnUrl);
            }
        }

        /// <summary>
        /// Редактирование внешних систем пользователя
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditUserExternalSystemAccount([FromQuery]string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var currentUserId = GetCurrentUserId();
            var externalSystemInfoViewModel = new ExternalSystemInfoViewModel();

            var externalSystemAccounts = await _externalSystemAccountService.GetUsersExternalSystemAccounts(currentUserId);
            var loginProviders = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!loginProviders.Any())
            {
                _logger.LogError("Отсутствуют логин-провайдеры внешних систем");
                Redirect(returnUrl);
            }

            if (externalSystemAccounts?.Count > 0)
            {
                externalSystemInfoViewModel.UserExternalSystemNames.AddRange(externalSystemAccounts.Select(s => s.ExternalSystemName));
            }

            externalSystemInfoViewModel.RemainExternalSystemNames
                .AddRange(loginProviders
                    .Where(s => !externalSystemInfoViewModel.UserExternalSystemNames
                        .Contains(s.Name))
                    .Select(s => s.Name));

            return View(externalSystemInfoViewModel);
        }

        /// <summary>
        /// Добавление внешней системы зарегистрированному пользователю
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult AddUserExternalSystemAccount(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(AddUserExternalSystemAccountCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        /// <summary>
        /// Удаление внешней системы зарегистрированному пользователю
        /// </summary>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserExternalSystemAccount(string provider, string returnUrl = null)
        {
            await _externalSystemAccountService.DeleteUsersExternalSystemAccount(GetCurrentUserId(), provider);
            return Redirect(returnUrl);
        }

        /// <summary>
        /// Добавление внешней системы зарегистрированному пользователю
        /// </summary>        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AddUserExternalSystemAccountCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            await _externalSystemAccountService.AddUsersExternalSystemAccount(GetCurrentUserId(), info.LoginProvider, info.ProviderKey);
            return Redirect(returnUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// ExternalLogin
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        /// <summary>
        /// ExternalLoginCallback
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return RedirectToAction(nameof(Login));
            }
            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(LoginWith2Fa), new { returnUrl });
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            // If the user does not have an account, then ask the user to create an account.
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;

            var externalSystemIdentifier = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            var currentUser = await _userManager.Users
                .Include(s => s.ExternalLogins)
                .FirstOrDefaultAsync(x => x.ExternalLogins
                .Any(s => s.ExternalUserName == externalSystemIdentifier && s.ExternalSystemName == info.LoginProvider));

            if (currentUser == null)
            {
                if (!AllowRegister)
                {
                    var loginViewModel = await BuildLoginViewModelAsync(returnUrl);
                    loginViewModel.ErrorMessage = $"Аккаунт {info.LoginProvider} не найден";
                    return View(nameof(Login), loginViewModel);
                }
                return View("ExternalLogin", new ExternalLoginViewModel { Login = email, ExternalSystemIdentifier = externalSystemIdentifier, ExternalSystemName = info.LoginProvider });
            }

            return await SignInAndRedirect(currentUser, returnUrl);
        }

        /// <summary>
        /// ExternalLoginConfirmation
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null, string loginProvider = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = loginProvider;

            if (ModelState.IsValid && ValidateCaptcha())
            {
                model.IsSucceed = true;
                model.ErrorMessages = new List<string>();
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                var userAndError = await CreateUser(model.Login, string.Empty, model.Phone, model.ExternalSystemIdentifier, model.ExternalSystemName);
                var user = userAndError.Item1;
                var error = userAndError.Item2;
                if (!string.IsNullOrEmpty(error))
                {
                    model.IsSucceed = false;
                    model.ErrorMessages.Add(error);
                    return View(nameof(ExternalLogin), model);
                }
                if (user == null)
                {
                    model.IsSucceed = false;
                    model.ErrorMessages.Add("Ошибка при создании пользователя. Обратитесь к системному администратору");
                    return View(nameof(ExternalLogin), model);
                }

                if (model.IsSucceed && !model.ErrorMessages.Any())
                {
                    //TODO: GenerateEmailConfirmationTokenAsync
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    //await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    return await SignInAndRedirect(user, returnUrl);
                }
            }

            return View(nameof(ExternalLogin), model);
        }

        /// <summary>
        /// Lockout
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        /// <summary>
        /// LoginWith2Fa
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2Fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new ApplicationException("Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2FaViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        /// <summary>
        /// LoginWith2Fa
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2Fa(LoginWith2FaViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }

            ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            return View();
        }

        /// <summary>
        /// LoginWithRecoveryCode
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException("Unable to load two-factor authentication user.");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        /// <summary>
        /// LoginWithRecoveryCode
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException("Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }

            ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
            return View();
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await _signInManager.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return View("LoggedOut", vm);
        }

        /// <summary>
        /// Show register page
        /// </summary>
        [HttpGet]
        public IActionResult Register(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl) && TempData["ReturnUrl"] != null && !string.IsNullOrEmpty(TempData["ReturnUrl"].ToString()))
            {
                returnUrl = TempData["ReturnUrl"].ToString();
            }

            if (!AllowRegister)
            {
                return RedirectToLocal(returnUrl);
            }

            ViewData["ReturnUrl"] = returnUrl;
            var registerViewModel = new RegisterViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(registerViewModel);
        }

        /// <summary>
        /// Post register page
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            var validCaptcha = ValidateCaptcha();
            if (ModelState.IsValid && validCaptcha)
            {
                registerViewModel.IsSucceed = true;
                registerViewModel.ErrorMessages = new List<string>();

                var userAndError = await CreateUser(registerViewModel.Login, registerViewModel.Password, registerViewModel.Phone);
                var user = userAndError.Item1;
                var error = userAndError.Item2;
                if (!string.IsNullOrEmpty(error))
                {
                    registerViewModel.IsSucceed = false;
                    registerViewModel.ErrorMessages.Add(error);
                    return View(registerViewModel);
                }
                if (user == null)
                {
                    registerViewModel.IsSucceed = false;
                    registerViewModel.ErrorMessages.Add("Ошибка при создании пользователя. Обратитесь к системному администратору");
                    return View(registerViewModel);
                }

                if (registerViewModel.IsSucceed && !registerViewModel.ErrorMessages.Any())
                {
                    //TODO: GenerateEmailConfirmationTokenAsync
                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    //await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    return await SignInAndRedirect(user, registerViewModel.ReturnUrl);
                }
            }
            else
            {
                registerViewModel.IsSucceed = false;
                registerViewModel.ErrorMessages = new List<string>();
                foreach (var kvp in ModelState)
                {
                    var message = kvp.Value.Errors.FirstOrDefault()?.ErrorMessage;
                    if (!string.IsNullOrEmpty(message))
                    {
                        registerViewModel.ErrorMessages.Add(message);
                    }
                }
                if (!validCaptcha)
                {
                    registerViewModel.ErrorMessages.Add("Докажите, что Вы не робот");
                }
            }
            return View(registerViewModel);
        }

        #region Private   

        private bool ValidateCaptcha()
        {
            var response = Request.Form[_configuration["CSP:response"]];
            var secretKey = _configuration["CSP:secretkey"];
            var client = new WebClient();
            var jsonResult = client.DownloadString(string.Format(_configuration["CSP:recaptchaUrl"], secretKey, response));
            var result = JsonConvert.DeserializeObject<ValidCaptcha>(jsonResult);
            if (result == null || result.Success == false)
            {
                return false;
            }
            return true;
        }

        private async Task<(ApplicationUser, string)> CreateUser(string login,
            string password,
            string phone,
            string externalSystemIdentifier = null, string externalSystemName = null)
        {
            ApplicationUser user = null;
            string error = string.Empty;
            try
            {
                user = await _userManager.FindByNameAsync(login);
                if (user != null)
                {
                    return (null, $"Пользователь с email {login} уже существует");
                }

                var userDto = new CreateUserDto
                {
                    UserName = login,
                    EmailConfirmed = false,
                    PhoneNumber = phone,
                    PhoneNumberConfirmed = false,
                    ExternalSystemIdentifier = externalSystemIdentifier,
                    ExternalSystemName = externalSystemName,
                    Password = password
                };

                user = await _createUserService.CreateUser(userDto);
            }
            catch (Exception e)
            {
                error = e.Message;
                return (user, error);
            }

            return (user, error);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private async Task<IActionResult> SignInAndRedirect(ApplicationUser user, string returnUrl)
        {
            // issue authentication cookie with subject ID and username
            await _signInManager.SignInAsync(user, isPersistent: false);

            // make sure the returnUrl is still valid, and if so redirect back to authorize endpoint or a local page
            // the IsLocalUrl check is only necessary if you want to support additional local pages, otherwise IsValidReturnUrl is more strict
            if (_interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return Redirect(Url.Content("~/"));
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            AuthorizationRequest context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null)
            {
                // this is meant to short circuit the UI and only trigger the one external IdP
                return new LoginViewModel
                {
                    AllowRegister = AllowRegister,
                    EnableLocalLogin = false,
                    ReturnUrl = returnUrl,
                    Login = context.LoginHint,
                    ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } },
                    ClientId = context.ClientId
                };
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null ||
                            (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRegister = AllowRegister,
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Login = context?.LoginHint,
                ExternalProviders = providers.ToArray(),
                RedirectToRegister = GetBoolWithName(context, "redirect_to_register"),
                ClientId = context?.ClientId
            };
        }

        private string ComposeErrorFromErrorList(IList<string> errors)
        {
            string result = string.Empty;
            if (errors.Count == 0)
            {
                return result;
            }
            foreach (var errorMessage in errors)
            {
                result += errorMessage + ";";
            }
            result.TrimEnd(';');
            return result;
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginViewModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Login = model.Login;
            vm.AllowRememberLogin = model.AllowRememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        private bool GetBoolWithName(AuthorizationRequest request, string name)
        {
            if (request?.Parameters == null || !request.Parameters.Any())
            {
                return false;
            }

            var requestParameter = request.Parameters[name];
            if (string.IsNullOrEmpty(requestParameter))
            {
                return false;
            }

            if (bool.TryParse(requestParameter, out var result))
            {
                return result;
            }

            return false;
        }

        private void ProcessAuthenticationProperties(LoginViewModel model)
        {
            //TODO: разобраться как работает
            /*
            // only set explicit expiration here if user chooses "remember me". 
            // otherwise we rely upon expiration configured in cookie middleware.
            AuthenticationProperties props = null;
            if (AccountOptions.AllowRememberLogin && model.AllowRememberLogin)
            {
                props = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                };
            };
            */
        }

        private string GetCurrentUserId()
        {
            var firstClaim = User?.Claims?.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(firstClaim))
            {
                return User?.Claims?.Where(c => c.Type == "sub").Select(c => c.Value)
                    .FirstOrDefault();
            }
            return firstClaim;
        }

        private bool AllowRegister
        {
            get
            {
                return bool.TryParse(_configuration["AllowRegister"], out var result) && result;
            }
        }
        #endregion
    }
}