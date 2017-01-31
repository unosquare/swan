namespace Unosquare.Swan.AspNetCore
{
    using Formatters;
    using Logger;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using Models;
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions methods to implement SWAN providers
    /// </summary>
    public static class Extensions
    {
        public const string JsonMimeType = "application/json";

        /// <summary>
        /// Setups the cookies.
        /// </summary>
        /// <param name="identityOptions">The identity options.</param>
        public static void SetupCookies(this IdentityOptions identityOptions)
        {
            identityOptions.Cookies.ApplicationCookie.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context =>
                {
                    // Skip the login
                    return Task.FromResult(0);
                }
            };
        }

        /// <summary>
        /// Uses the json exception handler.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseJsonExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500; // or another Status accordingly to Exception Type
                    context.Response.ContentType = JsonMimeType;
                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    await context.Response.WriteAsync(Json.Serialize(error?.Error ?? new Exception("Unhandled Exception")));
                });
            });
        }

        /// <summary>
        /// Adds the entity framework logger.
        /// </summary>
        /// <typeparam name="TDbContext">The type of the database context.</typeparam>
        /// <typeparam name="TLog">The type of the log.</typeparam>
        /// <param name="factory">The factory.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">factory</exception>
        public static ILoggerFactory AddEntityFramework<TDbContext, TLog>(this ILoggerFactory factory, IServiceProvider serviceProvider, Func<string, LogLevel, bool> filter = null)
            where TDbContext : DbContext
            where TLog : LogEntry, new()
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            factory.AddProvider(new EntityFrameworkLoggerProvider<TDbContext, TLog>(serviceProvider, filter));
            return factory;
        }

        /// <summary>
        /// Uses the bearer token provider.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="validationParameter">The validation parameter.</param>
        /// <param name="identityResolver">The identity resolver.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseBearerTokenProvider(this IApplicationBuilder app, TokenValidationParameters validationParameter, Func<string, string, string, string, Task<ClaimsIdentity>> identityResolver)
        {
            app.UseMiddleware<TokenProviderMiddleware>(Options.Create(new TokenProviderOptions
            {
                Audience = validationParameter.ValidAudience,
                Issuer = validationParameter.ValidIssuer,
                SigningCredentials = new SigningCredentials(validationParameter.IssuerSigningKey, SecurityAlgorithms.HmacSha256),
                IdentityResolver = identityResolver
            }));

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = validationParameter
            });

            return app;
        }

        /// <summary>
        /// Uses the fallback to redirect everything without extension.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="fallbackPath">The fallback path.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseFallback(this IApplicationBuilder app, string fallbackPath = "/index.html", Func<PathString, bool> ignoreCheck = null)
        {
            if (ignoreCheck == null)
                ignoreCheck = (path) => { return path.StartsWithSegments("/api") == false; };

            return app.Use(async (context, next) =>
            {
                await next();

                // If there's no available file and the request doesn't contain an extension, we're probably trying to access a page.
                // Rewrite request to use app root
                if (context.Response.StatusCode == (int)HttpStatusCode.NotFound
                    && ignoreCheck(context.Request.Path)
                    && !Path.HasExtension(context.Request.Path.Value))
                {
                    context.Request.Path = fallbackPath;
                    await next();
                }
            });
        }
    }
}