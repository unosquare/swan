namespace Unosquare.Swan.AspNetCore
{
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides options for <see cref="TokenProviderMiddleware"/>.
    /// </summary>
    public class TokenProviderOptions
    {
        /// <summary>
        /// The relative request path to listen on.
        /// </summary>
        /// <remarks>The default path is <c>/api/token</c>.</remarks>
        public string Path { get; set; } = "/api/token";

        /// <summary>
        ///  The Issuer (iss) claim for generated tokens.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// The Audience (aud) claim for the generated tokens.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [force HTTPS].
        /// </summary>
        public bool ForceHttps { get; set; } = true;

        /// <summary>
        /// The expiration time for the generated tokens.
        /// </summary>
        /// <remarks>The default is five minutes (300 seconds).</remarks>
        public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(20);

        /// <summary>
        /// The signing key to use when generating tokens.
        /// </summary>
        public SigningCredentials SigningCredentials { get; set; }

        /// <summary>
        /// Resolves a user identity given a username and password.
        /// </summary>
        public Func<string, string, string, string, Task<ClaimsIdentity>> IdentityResolver { get; set; }

        /// <summary>
        /// Generates a random value (nonce) for each generated token.
        /// </summary>
        /// <remarks>The default nonce is a random GUID.</remarks>
        public Func<Task<string>> NonceGenerator { get; set; } = () => Task.FromResult(Guid.NewGuid().ToString());

        /// <summary>
        /// Resolves the claims from a user.
        /// </summary>
        public Func<ClaimsIdentity, Task<Claim[]>> ClaimResolver { get; set; } = (identity) => Task.FromResult<Claim[]>(null);

        /// <summary>
        /// Resolves a bearer token response
        /// </summary>
        public Func<ClaimsIdentity, Dictionary<string, object>, Task<Dictionary<string, object>>> BearerTokenResolver { get; set; } = (identity, input) => Task.FromResult(input);
    }
}