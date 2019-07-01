using IdentityServer4.Models;
using System.Collections.Generic;

namespace SevSharks.Identity.WebUI
{
    public static class CustomApiResources
    {
        public static class CustomScopes
        {
            public const string GatewayScopeName = "sevsharks_gateway_api";
            public const string SignalrScopeName = "sevsharks_signalr_web";
        }

        public class GatewayScope : Scope
        {
            public GatewayScope()
            {
                Name = CustomScopes.GatewayScopeName;
                DisplayName = "ВместеНаТакси";
                Required = true;
            }
        }

        public class SignalrScope : Scope
        {
            public SignalrScope()
            {
                Name = CustomScopes.SignalrScopeName;
                DisplayName = "Уведомления от ВместеНаТакси";
                Required = true;
            }
        }

        public class Gateway : ApiResource
        {
            public Gateway()
            {
                Name = CustomScopes.GatewayScopeName;
                DisplayName = "Gateway API Resource";
                Scopes = new List<Scope> { new GatewayScope() };
            }
        }

        public class SignalR : ApiResource
        {
            public SignalR()
            {
                Name = CustomScopes.SignalrScopeName;
                DisplayName = "SignalR Web Resource";
                Scopes = new List<Scope> { new SignalrScope() };
            }
        }
    }
}
