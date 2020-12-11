using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PDS.Cestovatelia.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private AuthService _auth;
        public CustomAuthStateProvider(AuthService auth)
        {
            _auth = auth;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var userInfo = await _auth.GetCurrentUser();

            if (!string.IsNullOrWhiteSpace(userInfo.Name)) {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, userInfo.Nickname),
                    new Claim(ClaimTypes.Role, userInfo.Role.ToString())
                }, "Custom authentication type");

                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }

            return new AuthenticationState(new ClaimsPrincipal());
        }
    }
}
