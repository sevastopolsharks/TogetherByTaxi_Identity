using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SevSharks.Identity.WebUI.esiaconnection
{
    /// <summary>
    /// EsiaAuthenticationHandler
    /// </summary>
    public class EsiaAuthenticationHandler : OAuthHandler<EsiaAuthenticationOptions>
    {
        private const string SiteReturnAdditional = "siteReturnAdditional";
        private const string RedirectName = ".redirect";

        /// <summary>
        ///     Constructor
        /// </summary>
        public EsiaAuthenticationHandler(IOptionsMonitor<EsiaAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        /// <summary>
        /// CreateTicketAsync
        /// </summary>
        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity,
            AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            var parts = tokens.AccessToken.Split('.');
            if (parts.Length < 2)
            {
                throw new HttpRequestException("An error occurred while retrieving the user profile. Incorrect access_token");
            }

            string infoFromToken = Encoding.UTF8.GetString(Base64Decode(parts[1]));
            var esiaInfo = JsonConvert.DeserializeObject<EsiaInfo>(infoFromToken);
            var userIdentifier = esiaInfo.SbjId;
            if (string.IsNullOrEmpty(userIdentifier))
            {
                throw new HttpRequestException("An error occurred while retrieving the user profile. Cannot get SbjId from token");
            }

            var address = QueryHelpers.AddQueryString(string.Format(Options.UserInformationEndpoint, userIdentifier), new Dictionary<string, string>());
            var response = await GetAsync(address, tokens.AccessToken);
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError("An error occurred while retrieving the user profile: the remote server " +
                                "returned a {Status} response with the following payload: {Headers} {Body}.",
                    /* Status: */ response.StatusCode,
                    /* Headers: */ response.Headers.ToString(),
                    /* Body: */ content);

                throw new HttpRequestException("An error occurred while retrieving the user profile.");
            }

            var payload = JObject.Parse(content);

            AddOptionalClaim(identity, ClaimTypes.NameIdentifier, userIdentifier, Options.ClaimsIssuer);
            AddOptionalClaim(identity, ClaimTypes.GivenName, payload.Value<string>("firstName"), Options.ClaimsIssuer);
            AddOptionalClaim(identity, ClaimTypes.Surname, payload.Value<string>("lastName"), Options.ClaimsIssuer);
            AddOptionalClaim(identity, ClaimTypes.Email, payload.Value<string>("email"), Options.ClaimsIssuer);

            var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme,
                Options, Backchannel, tokens, payload);

            context.RunClaimActions();

            await Options.Events.CreatingTicket(context);

            return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            var oauthHandler = this;
            var query = oauthHandler.Request.Query;
            var properties = new AuthenticationProperties
            {
                Items =
                {
                    {"LoginProvider", Options.ClaimsIssuer}
                }
            };
            var redirectValue = "/Account/ExternalLoginCallback";
            if (query.ContainsKey(SiteReturnAdditional))
            {
                redirectValue = query[SiteReturnAdditional];
                //properties = oauthHandler.Options.StateDataFormat.Unprotect();
            }
            properties.Items.Add(RedirectName, redirectValue);
            var error = query["error"];
            if (!StringValues.IsNullOrEmpty(error))
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(error);
                var stringValues3 = query["error_description"];
                if (!StringValues.IsNullOrEmpty(stringValues3))
                {
                    stringBuilder.Append(";Description=").Append(stringValues3);
                }

                var stringValues4 = query["error_uri"];
                if (!StringValues.IsNullOrEmpty(stringValues4))
                {
                    stringBuilder.Append(";Uri=").Append(stringValues4);
                }

                return HandleRequestResult.Fail(stringBuilder.ToString(), properties);
            }
            var code = query["code"];
            if (StringValues.IsNullOrEmpty(code))
            {
                return HandleRequestResult.Fail("Code was not found.", properties);
            }

            var tokens = await GetTokenByAuthCodeAsync(code);
            if (tokens.Error != null)
            {
                return HandleRequestResult.Fail(tokens.Error, properties);
            }

            if (string.IsNullOrEmpty(tokens.AccessToken))
            {
                return HandleRequestResult.Fail("Failed to retrieve access token.", properties);
            }

            var identity = new ClaimsIdentity(oauthHandler.ClaimsIssuer);
            if (oauthHandler.Options.SaveTokens)
            {
                var authenticationTokenList = new List<AuthenticationToken>
                {
                    new AuthenticationToken
                    {
                        Name = "access_token",
                        Value = tokens.AccessToken
                    }
                };
                if (!string.IsNullOrEmpty(tokens.RefreshToken))
                {
                    authenticationTokenList.Add(new AuthenticationToken
                    {
                        Name = "refresh_token",
                        Value = tokens.RefreshToken
                    });
                }

                if (!string.IsNullOrEmpty(tokens.TokenType))
                {
                    authenticationTokenList.Add(new AuthenticationToken
                    {
                        Name = "token_type",
                        Value = tokens.TokenType
                    });
                }

                if (!string.IsNullOrEmpty(tokens.ExpiresIn) && int.TryParse(tokens.ExpiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                {
                    var dateTimeOffset = oauthHandler.Clock.UtcNow + TimeSpan.FromSeconds(result);
                    authenticationTokenList.Add(new AuthenticationToken
                    {
                        Name = "expires_at",
                        Value = dateTimeOffset.ToString("o", CultureInfo.InvariantCulture)
                    });
                }
                properties.StoreTokens(authenticationTokenList);
            }
            var ticketAsync = await oauthHandler.CreateTicketAsync(identity, properties, tokens);
            return ticketAsync == null ? 
                HandleRequestResult.Fail("Failed to retrieve user information from remote server.", properties) : 
                HandleRequestResult.Success(ticketAsync);
        }

        /// <summary>
        /// BuildChallengeUrl
        /// </summary>
        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            var parameter = properties.GetParameter<ICollection<string>>(OAuthChallengeProperties.ScopeKey);
            var scopes = parameter != null ? FormatScope(parameter) : FormatScope();
            //string siteState = Options.StateDataFormat.Protect(properties);
            string siteState = properties.Items.ContainsKey(RedirectName) ? properties.Items[RedirectName] : "";
            if (!string.IsNullOrEmpty(siteState))
            {
                var encode = WebUtility.UrlEncode(siteState);
                redirectUri += $"?{SiteReturnAdditional}={encode}";
            }

            var timestamp = GetTimeStamp();
            var state = GetState();
            var signMessage = $"{scopes}{timestamp}{Options.ClientId}{state}";
            var clientSecret = SignString(signMessage).Result;
            var res = QueryHelpers.AddQueryString(Options.AuthorizationEndpoint, new Dictionary<string, string>
            {
                {
                    "client_id",
                    Options.ClientId
                },
                {
                    "client_secret",
                    clientSecret
                },
                {
                    "redirect_uri",
                    redirectUri
                },
                {
                    "scope",
                    scopes
                },
                {
                    "response_type",
                    "code"
                },
                {
                    "state",
                    state
                },
                {
                    "access_type",
                    "online"
                },
                {
                    "timestamp",
                    timestamp
                }
            });
            return res;
        }

        private string GetState()
        {
            return Guid.NewGuid().ToString("D");
        }

        private string GetTimeStamp()
        {
            return DateTime.UtcNow.ToString("yyyy.MM.dd HH:mm:ss +0000");
        }

        private async Task<HttpResponseMessage> GetAsync(string url, string authToken)
        {
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(authToken))
                {
                    //Add the authorization header
                    client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("Bearer " + authToken);
                }

                return await client.GetAsync(url);
            }
        }

        private async Task<OAuthTokenResponse> GetTokenByAuthCodeAsync(string authCode)
        {
            if (string.IsNullOrEmpty(authCode))
            {
                throw new ArgumentNullException(nameof(authCode));
            }

            var timestamp = GetTimeStamp();
            var scopes = FormatScope(Options.Scope);
            var state = GetState();
            var signMessage = $"{scopes}{timestamp}{Options.ClientId}{state}";
            var clientSecret = SignString(signMessage).Result;

            var requestParameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("client_id", Options.ClientId),
                new KeyValuePair<string, string>("code", authCode),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("state", state),
                new KeyValuePair<string, string>("scope", scopes),
                new KeyValuePair<string, string>("timestamp", timestamp),
                new KeyValuePair<string, string>("token_type", "Bearer"),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", Options.CallbackPath)
            };

            var requestContent = new FormUrlEncodedContent(requestParameters);

            try
            {
                using (var response = await Backchannel.PostAsync(Options.TokenEndpoint, requestContent, Context.RequestAborted))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"ЕСИА вернул неуспешный код code: '{response.StatusCode}'.");
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    return OAuthTokenResponse.Success(JObject.Parse(content));
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        private byte[] Base64Decode(string input)
        {
            input = input.Replace('-', '+').Replace('_', '/');

            switch (input.Length % 4)
            {
                case 0:
                    break;
                case 2:
                    input = $"{input}==";
                    break;
                case 3:
                    input = $"{input}=";
                    break;
                default:
                    throw new Exception("Illegal base64url string!");
            }

            return Convert.FromBase64String(input);
        }

        private void AddOptionalClaim(ClaimsIdentity identity, string type, string value, string issuer)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            // Don't update the identity if the claim cannot be safely added.
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(value)) return;

            identity.AddClaim(new Claim(type, value, ClaimValueTypes.String, issuer ?? ClaimsIdentity.DefaultIssuer));
        }

        private Task<string> SignString(string strToSign)
        {
            //TODO: implement signature for byte array
            throw new Exception("Signature is not implemented");
            //var bytes = Encoding.UTF8.GetBytes(strToSign);
            //var result = await _cryptographyService.Sign(bytes, Options.InformationSystemThumbprint, true);
            //if (!result.IsSuccess) throw new Exception("Не получилось подписать");
            //var clientSecret = Encode(result.Result);
            //return clientSecret;
        }

        private string Encode(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.Length < 1)
            {
                return string.Empty;
            }

            int endPos;
            var base64Str = Convert.ToBase64String(input);

            for (endPos = base64Str.Length; endPos > 0; endPos--)
            {
                if (base64Str[endPos - 1] != '=')
                {
                    break;
                }
            }

            var base64Chars = new char[endPos + 1];
            base64Chars[endPos] = (char) ('0' + base64Str.Length - endPos);

            for (var iter = 0; iter < endPos; iter++)
            {
                var c = base64Str[iter];

                switch (c)
                {
                    case '+':
                        base64Chars[iter] = '-';
                        break;

                    case '/':
                        base64Chars[iter] = '_';
                        break;

                    case '=':
                        base64Chars[iter] = c;
                        break;

                    default:
                        base64Chars[iter] = c;
                        break;
                }
            }

            return new string(base64Chars);
        }
    }
}