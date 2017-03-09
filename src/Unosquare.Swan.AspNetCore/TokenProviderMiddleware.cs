namespace Unosquare.Swan.AspNetCore
{
    using Formatters;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    /// <summary>
    /// Token generator middleware component which is added to an HTTP pipeline.
    /// </summary>
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenProviderOptions _options;
        private readonly ILogger _logger;
        private readonly Dictionary<Guid, JwtSecurityToken> _refreshTokens = new Dictionary<Guid, JwtSecurityToken>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenProviderMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        /// <param name="options">The options.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public TokenProviderMiddleware(
            RequestDelegate next,
            IOptions<TokenProviderOptions> options,
            ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<TokenProviderMiddleware>();

            _options = options.Value;
            ThrowIfInvalidOptions(_options);
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task Invoke(HttpContext context)
        {
            // Check if we are getting a new token
            if (context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
            {
                context.Response.ContentType = Extensions.JsonMimeType;

                // Request must be POST with Content-Type: application/x-www-form-urlencoded 
                // and HTTPS if options requires it
                if (!context.Request.Method.Equals("POST")
                    || !context.Request.HasFormContentType ||
                    (_options.ForceHttps && context.Request.IsHttps == false))
                {
                    context.Response.StatusCode = 400;
                    return context.Response.WriteAsync(SerializeError("Bad request."));
                }

                _logger.LogInformation($"Handling request: {context.Request.Path}");

                return GenerateToken(context);
            }

            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var bearerToken = context.Request.Headers["Authorization"].FirstOrDefault(x => x.StartsWith("Bearer"));

                if (bearerToken == null) return _next(context);
                bearerToken = bearerToken.Split(' ')[1];
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadToken(bearerToken) as JwtSecurityToken;

                if (DateTime.UtcNow > token?.ValidTo)
                {
                    context.Response.ContentType = Extensions.JsonMimeType;
                    context.Response.StatusCode = 401;
                    return context.Response.WriteAsync(SerializeError("The access token provided has expired.", "invalid_token"));
                }
            }

            return _next(context);
        }

        private async Task GenerateToken(HttpContext context)
        {
            JwtSecurityToken jwt;
            ClaimsIdentity identity = null;
            var now = DateTime.UtcNow;
            var grantType = context.Request.Form["grant_type"];

            if (grantType == "refresh_token")
            {
                var refreshToken = context.Request.Form["refresh_token"];
                Guid guidToken;

                if (Guid.TryParse(refreshToken, out guidToken) == false || _refreshTokens.ContainsKey(guidToken) == false)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync(SerializeError("Invalid refresh token."));

                    _logger.LogDebug($"Invalid refresh token ({refreshToken})");
                    return;
                }

                var claims = _refreshTokens[guidToken].Claims.ToList();
                claims.Remove(claims.First(x => x.Type == JwtRegisteredClaimNames.Iat));
                claims.Add(new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixEpochDate().ToString(), ClaimValueTypes.Integer64));

                jwt = new JwtSecurityToken(
                    issuer: _options.Issuer,
                    audience: _options.Audience,
                    claims: claims,
                    notBefore: now,
                    expires: now.Add(_options.Expiration),
                    signingCredentials: _options.SigningCredentials);

                _refreshTokens.Remove(guidToken);
            }
            else
            {
                var username = context.Request.Form["username"];
                var password = context.Request.Form["password"];
                var clientId = context.Request.Form["client_id"];

                identity = await _options.IdentityResolver(username, password, grantType, clientId);

                if (identity == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync(SerializeError("Invalid username or password."));

                    _logger.LogDebug($"Invalid username ({username}) or password");
                    return;
                }

                _logger.LogDebug($"Valid username ({username})");

                // Specifically add the jti (nonce), iat (issued timestamp), and sub (subject/user) claims.
                // You can add other claims here, if you want:
                var claims = identity.Claims.ToList();

                claims.AddRange(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(JwtRegisteredClaimNames.Jti, await _options.NonceGenerator()),
                    new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixEpochDate().ToString(), ClaimValueTypes.Integer64)
                });

                //Add claim role
                var additionalClaims = await _options.ClaimResolver(identity);

                if (additionalClaims != null) claims.AddRange(additionalClaims);

                // Create the JWT and write it to a string
                jwt = new JwtSecurityToken(
                    issuer: _options.Issuer,
                    audience: _options.Audience,
                    claims: claims,
                    notBefore: now,
                    expires: now.Add(_options.Expiration),
                    signingCredentials: _options.SigningCredentials);
            }

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var refreshTokenGuid = Guid.NewGuid();

            var responseInfo = new Dictionary<string, object>
            {
                { "access_token", encodedJwt },
                { "expires_in", (int)_options.Expiration.TotalSeconds },
                { "refresh_token", refreshTokenGuid }
            };

            _refreshTokens.Add(refreshTokenGuid, jwt);

            await context.Response.WriteAsync(Json.Serialize(await _options.BearerTokenResolver(identity, responseInfo)));
        }

        private static void ThrowIfInvalidOptions(TokenProviderOptions options)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Path));
            }

            if (string.IsNullOrEmpty(options.Issuer))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Issuer));
            }

            if (string.IsNullOrEmpty(options.Audience))
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.Audience));
            }

            if (options.Expiration == TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(TokenProviderOptions.Expiration));
            }

            if (options.IdentityResolver == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.IdentityResolver));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.SigningCredentials));
            }

            if (options.NonceGenerator == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.NonceGenerator));
            }

            if (options.IdentityResolver == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.IdentityResolver));
            }

            if (options.BearerTokenResolver == null)
            {
                throw new ArgumentNullException(nameof(TokenProviderOptions.BearerTokenResolver));
            }
        }

        /// <summary>
        /// Serializes the error.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        private static string SerializeError(string description, string error = "invalid_grant")
        {
            return Json.Serialize(new
            {
                error = error,
                error_description = description
            });
        }
    }
}