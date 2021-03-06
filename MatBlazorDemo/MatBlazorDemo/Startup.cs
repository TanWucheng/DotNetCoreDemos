using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MatBlazorDemo.Data;
using Microsoft.EntityFrameworkCore;
using MatBlazorDemo.Services.Interfaces;
using MatBlazorDemo.Services;
using System.Net.Http;
using System.Linq;
using System;
using Microsoft.AspNetCore.Components;
using MatBlazor;
using System.IO;
using Microsoft.Extensions.FileProviders;

namespace MatBlazorDemo
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
            services.AddRazorPages();
            services.AddServerSideBlazor();

            // 注册DBContext
            services.AddDbContext<EfDbContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("EfDbContext"));
            });

            // 瞬时方式注入业务模块
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IIdentityService, IdentityService>();

            // 单例模式注入业务模块
            services.AddSingleton<WeatherForecastService>();
            services.AddSingleton<VerifyCodeService>();

            // 解决MatTable对HttpClient的依赖
            if (services.All(x => x.ServiceType != typeof(HttpClient)))
            {
                services.AddScoped(
                    s =>
                    {
                        var navigationManager = s.GetRequiredService<NavigationManager>();
                        return new HttpClient
                        {
                            BaseAddress = new Uri(navigationManager.BaseUri)
                        };
                    });
            }

            // MatToast
            services.AddMatToaster(config =>
            {
                config.Position = MatToastPosition.BottomFullWidth;
                config.PreventDuplicates = true;
                config.NewestOnTop = true;
                config.ShowCloseButton = true;
                config.MaximumOpacity = 95;
                config.VisibleStateDuration = 3000;
                config.ShowProgressBar = true;
            });

            // 注册BlazorPlus相关服务
            services.AddHttpContextAccessor();
            services.AddScoped<BlazorPlus.BlazorSession>();

            // 注册剪贴板服务
            // services.AddClipboard();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(
            Path.Combine(env.ContentRootPath, "MyStaticFiles")),
                RequestPath = "/StaticFiles",
                EnableDirectoryBrowsing = true
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                //Install-BlazorPlus
                endpoints.Map("/_blazorplus_handler", BlazorPlus.BlazorSession.ProcessRequestAsync);

                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
