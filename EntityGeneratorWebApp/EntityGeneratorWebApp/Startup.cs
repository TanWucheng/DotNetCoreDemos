using System;
using System.IO;
using System.Text;
using DaoLibrary.Entities;
using DaoLibrary.Repositories;
using EntityGeneratorWebApp.Config;
using EntityGeneratorWebApp.Models;
using EntityGeneratorWebApp.Utils;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;

namespace EntityGeneratorWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            //����log4net��־�����ļ�
            LoggerRepository = LogManager.CreateRepository("NETCoreRepository");
            XmlConfigurator.Configure(LoggerRepository, new FileInfo("log4net.config"));
        }

        public static ILoggerRepository LoggerRepository { get; set; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });
            // ����Action���沶��ȫ���쳣
            services.AddControllers(configure => { configure.Filters.Add<HttpGlobalExceptionFilter>(); });
            // ע��Configuration����
            services.Configure<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"))
                .Configure<RsaKeyPair>(Configuration.GetSection("RsaKeyPair"));
            // ע��ҵ��ģ��
            services.AddTransient<ISqlServerRepository, SqlServerRepository>();
            services.AddTransient<IMySqlRepository, MySqlRepository>();
            services.AddTransient<IAuthRepository, AuthRepository>();

            // ע��Swagger������
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Entity Generator Asp.Net Core WebApi",
                    Version = "v1",
                    Description = "���ݿ�ʵ����������WebApi",
                    Contact = new OpenApiContact
                    {
                        Name = "̷���",
                        Email = "tanwucheng@outlook.com",
                        Url = new Uri("https://github.com/tanwucheng")
                    }
                });

                var xmlPath = Path.Combine(AppContext.BaseDirectory, "EntityGeneratorWebApp.xml");
                options.IncludeXmlComments(xmlPath);

                var scheme = new OpenApiSecurityScheme
                {
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Description = "Bearer Token"
                };
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, scheme);

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
            });

            // ���jwt��֤��
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, //�Ƿ���֤Issuer
                        ValidateAudience = true, //�Ƿ���֤Audience
                        ValidateLifetime = true, //�Ƿ���֤ʧЧʱ��
                        ValidateIssuerSigningKey = true, //�Ƿ���֤SecurityKey
                        ValidAudience = "ten", //Audience
                        ValidIssuer = "̷���", //Issuer���������ǰ��ǩ��jwt������һ��
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes("encrypt_the_validate_site_key")) //�õ�SecurityKey
                    };
                });

            services.AddMvc()
                .AddMvcLocalization()
                .AddViewLocalization()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // ���������ʾ����
            app.UseErrorHandling();

            // ������֤
            app.UseAuthentication();
            app.UseAuthorization();

            // �����м���Խ����ɵ�Swagger����ΪJSON�˵�
            app.UseSwagger();

            // ������빫������ʽ�ĵ�������ѡ�����Swagger-ui�м������ָ������֧������Swagger JSON�˵㡣
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Entity Generator Asp.Net Core WebApi V1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("encrypt_the_validate_site_key"));
            var options = new TokenGenerateOption
            {
                Path = "/token",
                Audience = "ten",
                Issuer = "̷���",
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                Expiration = TimeSpan.FromDays(7)
            };

            var userAuthRepository = app.ApplicationServices.GetService<IAuthRepository>();
            var keyPairs = Configuration.GetSection("RsaKeyPair").GetChildren();
            var privateKey = "";
            foreach (var item in keyPairs)
            {
                if (item.Key != "PrivateKey") continue;
                privateKey = item.Value;
                break;
            }
            var tokenGenerator = new TokenGenerator(options, userAuthRepository, privateKey);
            app.Map(options.Path, tokenGenerator.GenerateTokenAsync);
        }
    }
}