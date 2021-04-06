// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Configuration.DependencyInjection;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Duende.IdentityServer.Hosting.DynamicProviders
{
    class DynamicAuthenticationSchemeProvider : IAuthenticationSchemeProvider
    {
        private readonly IAuthenticationSchemeProvider _inner;
        private readonly DynamicProviderOptions _options;
        private readonly IIdentityProviderStore _identityProviderStore;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DynamicAuthenticationSchemeProvider> _logger;

        public DynamicAuthenticationSchemeProvider(
            Decorator<IAuthenticationSchemeProvider> inner,
            DynamicProviderOptions options,
            IIdentityProviderStore identityProviderStore,
            IHttpContextAccessor httpContextAccessor,
            ILogger<DynamicAuthenticationSchemeProvider> logger)
        {
            _inner = inner.Instance;
            _options = options;
            _identityProviderStore = identityProviderStore;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public void AddScheme(AuthenticationScheme scheme)
        {
            _inner.AddScheme(scheme);
        }

        public Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync()
        {
            return _inner.GetDefaultAuthenticateSchemeAsync();
        }

        public Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync()
        {
            return _inner.GetDefaultChallengeSchemeAsync();
        }

        public Task<AuthenticationScheme> GetDefaultForbidSchemeAsync()
        {
            return _inner.GetDefaultForbidSchemeAsync();
        }

        public Task<AuthenticationScheme> GetDefaultSignInSchemeAsync()
        {
            return _inner.GetDefaultSignInSchemeAsync();
        }

        public Task<AuthenticationScheme> GetDefaultSignOutSchemeAsync()
        {
            return _inner.GetDefaultSignOutSchemeAsync();
        }

        public Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
        {
            return _inner.GetAllSchemesAsync();
        }

        public Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync()
        {
            return _inner.GetRequestHandlerSchemesAsync();
        }

        public async Task<AuthenticationScheme> GetSchemeAsync(string name)
        {
            var scheme = await _inner.GetSchemeAsync(name);

            if (scheme == null)
            {
                return await GetDynamicSchemeAsync(name);
            }

            return scheme;
        }

        public void RemoveScheme(string name)
        {
            _inner.RemoveScheme(name);
        }


        private async Task<AuthenticationScheme> GetDynamicSchemeAsync(string name)
        {
            // this has to be here because the regular authenticaiton middleware accepts IAuthenticationSchemeProvider
            // as a ctor param, not an Invoke param, which makes it a singleton. Our DynamicAuthenticationSchemeCache is
            // scoped in DI.
            var cache = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<DynamicAuthenticationSchemeCache>();

            var dynamicScheme = cache.Get(name);
            if (dynamicScheme == null)
            {
                var idp = await _identityProviderStore.GetBySchemeAsync(name);
                if (idp != null && idp.Enabled)
                {
                    var providerType = _options.FindProviderType(idp.Type);
                    if (providerType != null)
                    {
                        dynamicScheme = new DynamicAuthenticationScheme(idp, providerType.HandlerType);
                        cache.Add(name, dynamicScheme);
                    }
                }
            }

            return dynamicScheme;
        }
    }
}
