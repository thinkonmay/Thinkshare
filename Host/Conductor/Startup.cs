using Microsoft.AspNetCore.Builder;
using DbSchema.CachedState;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Conductor.Services;
using DbSchema.SystemDb.Data;
using Conductor.Interfaces;
using SharedHost.Models.User;
using System;
using System.IO;
using System.Reflection;
using SharedHost;
using SharedHost.Logging;
using SharedHost.Auth;
using Conductor.Hubs;

namespace Conductor
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
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin());
            });

            //for postgresql
            services.AddDbContext<GlobalDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("PostgresqlConnection")),
                ServiceLifetime.Transient
            );
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("Redis");
                options.InstanceName = Configuration.GetConnectionString("RedisInstanceName");
            });
            services.AddDefaultIdentity<UserAccount>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole<int>>()
                .AddEntityFrameworkStores<GlobalDbContext>();

            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Host",
                    Version =
                    "v1"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                    }
                }); 
            });
            services.AddMvc();
            services.AddTransient<IClientHub,ClientHub>();
            services.AddTransient<IWorkerCommnader,WorkerCommander>();
            services.AddTransient<IGlobalStateStore,GlobalStateStore>();
            services.AddTransient<IClusterRBAC,ClusterRBAC>();
            services.AddSingleton<ILog, Log>();

            services.Configure<SystemConfig>(Configuration.GetSection("SystemConfig"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "JWTAuthDemo v1"));

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials()
                .SetIsOriginAllowed(origin => true)); // allow any origin


            app.UseRouting();
            app.UseMiddleware<JwtMiddleware>();
            app.UseMiddleware<AuthorizeMiddleWare>();

            app.UseMiddleware<LoggingMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
