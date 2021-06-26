using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using websocketschat.Bot.Interfaces;
using websocketschat.Core.Repositories;
using websocketschat.Core.Services.Implementations;
using websocketschat.Core.Services.Interfaces;
using websocketschat.Database.Context;
using websocketschat.Database.Repositories;
using websocketschat.Web.BackgroundService;
using websocketschat.Web.Helpers;
using websocketschat.Web.Helpers.Auth;
using websocketschat.Web.Hubs;
using websocketschat.Web.Providers;
using websocketschat.Bot;

namespace websocketschat.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.Configure<AuthOptions>(Configuration.GetSection("AuthOptions"));

            // getting the connection string from docker-compose. 
            // If they don't exist, use the appsetting.json file.
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? 
                                   Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<NpgSqlContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddTransient<IUserRepository, UserRepository>();

            services.AddSingleton<UsersContext>();

            services.AddScoped<IUserService, UserService>();

            services.AddTransient<IBotManager, BotManager>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = Configuration.GetSection("AuthOptions").Get<AuthOptions>().Issuer,
                        ValidateAudience = true,
                        ValidAudience = Configuration.GetSection("AuthOptions").Get<AuthOptions>().Audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey
                        (
                            Encoding.ASCII.GetBytes(Configuration.GetSection("AuthOptions").Get<AuthOptions>().SecretKey)
                        ),
                        ValidateIssuerSigningKey = true,
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/chat")))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddCors();
            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
            services.AddControllers();
        //    services.AddHostedService<BotBackgroundService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors(builder => builder.AllowAnyOrigin()
                                          .AllowAnyHeader()
                                          .AllowAnyMethod());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chat");
            });

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetService<NpgSqlContext>().Database.Migrate();
            }
        }
    }
}
