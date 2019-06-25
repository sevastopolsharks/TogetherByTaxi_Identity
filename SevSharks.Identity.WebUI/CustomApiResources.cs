using IdentityServer4.Models;
using System.Collections.Generic;

namespace SevSharks.Identity.WebUI
{
    public static class CustomApiResources
    {
        public static class CustomScopes
        {
            public const string GatewayScopeName = "it2_gateway_api";
            public const string SignalrScopeName = "it2_signalr_web";
            public const string BiddingSignScopeName = "it2_bidding_sign_api";
            public const string EmployeeScopeName = "it2_employee_web";
        }

        public class GatewayScope : Scope
        {
            public GatewayScope()
            {
                Name = CustomScopes.GatewayScopeName;
                DisplayName = "Имущество";
                Required = true;
            }
        }

        public class SignalrScope : Scope
        {
            public SignalrScope()
            {
                Name = CustomScopes.SignalrScopeName;
                DisplayName = "Уведомления от Имущества";
                Required = true;
            }
        }

        public class BiddingSignScope : Scope
        {
            public BiddingSignScope()
            {
                Name = CustomScopes.BiddingSignScopeName;
                DisplayName = "Подпись для Имущества";
                Required = true;
            }
        }

        public class EmployeeScope : Scope
        {
            public EmployeeScope()
            {
                Name = CustomScopes.EmployeeScopeName;
                DisplayName = "Сотрудник";
                Required = true;
            }
        }

        public class Gateway : ApiResource
        {
            public Gateway()
            {
                Name = CustomScopes.GatewayScopeName;
                DisplayName = "IT2 Gateway API Resource";
                Scopes = new List<Scope> { new GatewayScope() };
            }
        }

        public class SignalR : ApiResource
        {
            public SignalR()
            {
                Name = CustomScopes.SignalrScopeName;
                DisplayName = "IT2 SignalR Web Resource";
                Scopes = new List<Scope> { new SignalrScope() };
            }
        }
        public class BiddingSign : ApiResource
        {
            public BiddingSign()
            {
                Name = CustomScopes.BiddingSignScopeName;
                DisplayName = "IT2 BiddingSign Web Resource";
                Scopes = new List<Scope> { new BiddingSignScope() };
            }
        }

        /// <summary>
        /// EmployeeApiResource
        /// </summary>
        public class EmployeeApiResource : ApiResource
        {
            /// <summary>
            /// EmployeeApiResource - constructor
            /// </summary>
            public EmployeeApiResource()
            {
                Name = CustomScopes.EmployeeScopeName;
                DisplayName = "IT2 Employee Web Resource";
                Scopes = new List<Scope> { new EmployeeScope() };
            }
        }
    }
}
