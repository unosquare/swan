namespace Unosquare.Swan.AspNetCore.Sample
{
    using AspNetCore;
    using Database;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;

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
            // Add framework services.
            services.AddDbContext<SampleDbContext>(options => options.UseSqlServer(Configuration["ConnectionString"]));
            services.AddIdentity<BasicUserStore, BasicRoleStore>(options => options.SetupCookies()).AddDefaultTokenProviders();

            services.AddOptions();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddEntityFramework<SampleDbContext, Models.LogEntry>(app.ApplicationServices);

            app.UseFallback();
            app.UseJsonExceptionHandler();

            app.UseIdentity();

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
                // TODO: Complete
                return Task.FromResult(new ClaimsIdentity());
            });

            app.UseMvc();
        }
    }
}