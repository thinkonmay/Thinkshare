using Microsoft.AspNetCore.Builder;
using SharedHost.Models.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using WorkerManager.Interfaces;
using WorkerManager.Services;
using System;
using System.IO;
using System.Reflection;
using WorkerManager.Middleware;
using SharedHost;


namespace WorkerManager
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

            var REDIS_IP =  Environment.GetEnvironmentVariable("REDIS_IP");

            if(REDIS_IP == null)
            {
                Console.WriteLine("Missing environment variable");
                Console.WriteLine("Using default connection string");

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = "localhost:6379";
                    options.InstanceName = "Cluster";
                });
            }
            else
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = REDIS_IP+":6379";
                    options.InstanceName = "Cluster";
                });
            }

            services.AddControllers();            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Host",
                    Version =
                    "v1"
                });

                var xmlFilePath = Path.Combine(AppContext.BaseDirectory,
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

                c.IncludeXmlComments(xmlFilePath);

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
            services.Configure<ClusterConfig>(Configuration.GetSection("ClusterConfig"));
            services.Configure<InstanceSetting>(Configuration.GetSection("InstanceSetting"));
            services.Configure<JwtOptions>(Configuration.GetSection("JwtOptions"));
            services.AddTransient<ITokenGenerator,TokenGenerator>();
            services.AddTransient<IClusterInfor,ClusterInfor>();
            services.AddTransient<ILocalStateStore, LocalStateStore>();
            services.AddSingleton<IConductorSocket,ConductorSocket>();
            services.AddSingleton<IWorkerNodePool,WorkerNodePool>();
            services.AddSingleton<IPortProxy,PortProxy>();
            services.AddMvc();
        
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {            
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "signalling v1"));
            
            // global cors policy
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true)); // allow any origin

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseMiddleware<ClusterJwtMiddleware>();
            app.UseMiddleware<ClusterAuthorizeMiddleware>();


            app.UseWebSockets();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

            });
        }
    }
}
