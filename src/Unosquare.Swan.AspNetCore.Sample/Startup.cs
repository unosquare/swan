namespace Unosquare.Swan.AspNetCore.Sample
{
    using AspNetCore;
    using Database;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Add framework services.
            services.AddDbContext<SampleDbContext>(options => options.UseSqlServer(Configuration["ConnectionString"]));
            // Setup a basic stores for Identity and remove Cookies redirects to /Login
            services.AddIdentity<BasicUserStore, BasicRoleStore>(options => options.SetupCookies()).AddDefaultTokenProviders();

            services.AddOptions();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddEntityFramework<SampleDbContext, Models.LogEntry>(app.ApplicationServices);

            // Redirect anything without extension to index.html
            app.UseFallback();
            // Response an exception as JSON at error
            app.UseJsonExceptionHandler();

            app.UseIdentity();

            // Use the bearer token provider and check Admin and Passw.ord as valid credentials
            app.UseBearerTokenProvider(new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["SymmetricSecurityKey"])),

                ValidateIssuer = true,
                ValidIssuer = "IdentityCore",

                ValidateAudience = true,
                ValidAudience = "Unosquare",

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            }, (username, password, grantType, clientId) =>
            {
                if (username != "Admin" || password != "Pass.word")
                    return Task.FromResult<ClaimsIdentity>(null);

                var claim = new ClaimsIdentity("Bearer");
                claim.AddClaim(new Claim(ClaimTypes.Name, username));

                return Task.FromResult(claim);
            }, (identity, obj) =>
            {
                // This action is optional
                obj["test"] = "OK";
                return Task.FromResult(obj);
            });

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseMvc();
            app.UseStaticFiles();
        }
    }
}