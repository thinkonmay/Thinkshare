using Microsoft.AspNetCore.Builder;
using DbSchema.SystemDb.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;
using SharedHost;
using SystemHub.Interfaces;
using SystemHub.Services;
using DbSchema.SystemDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DbSchema.CachedState;

namespace SystemHub
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin());
            });

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
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("Redis");
                options.InstanceName = Configuration.GetConnectionString("RedisInstanceName");
            });

            services.Configure<SystemConfig>(Configuration.GetSection("SystemConfig"));
            services.AddSingleton<IGlobalStateStore,GlobalStateStore>();
            services.AddSingleton<IClusterSocketPool, ClusterSocketPool>();
            services.AddSingleton<IUserSocketPool, UserSocketPool>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "signalling v1"));

            app.UseDefaultFiles();
            app.UseStaticFiles();
            // global cors policy
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true)); // allow any origin

            app.UseRouting();

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
