using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using Conductor.Services;
using DbSchema.SystemDb.Data;
using Conductor.Interfaces;
using SharedHost.Models.Auth;
using SharedHost.Models.User;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using SignalRChat.Hubs;
using System.Threading.Tasks;
using SharedHost;
using SharedHost.Auth;

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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("PostgresqlConnection")),
                ServiceLifetime.Transient
            );

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<UserAccount>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole<int>>()
                .AddEntityFrameworkStores<ApplicationDbContext>();


            services.AddSingleton(Configuration.GetSection("SystemConfig").Get<SystemConfig>());
            services.AddSignalR();
            services.AddTransient<IAdmin, Admin>();
            services.AddSingleton<ISlaveManagerSocket,SlaveManagerSocket>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
           // global cors policy
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyHeader()); // allow any origin

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseMiddleware<JwtMiddleware>();


            app.UseWebSockets();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                    
                endpoints.MapHub<AdminHub>("/AdminHub");
                endpoints.MapHub<ClientHub>("/ClientHub");
            });
        }
    }
}
