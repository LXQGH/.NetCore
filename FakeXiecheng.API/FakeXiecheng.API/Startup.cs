using FakeXiecheng.API.Database;
using FakeXiecheng.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FakeXiecheng.API
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
            //JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   var secretByte = Encoding.UTF8.GetBytes(Configuration["Authentication:SecretKey"]);
                   options.TokenValidationParameters = new TokenValidationParameters()
                   {
                       ValidateIssuer = true,
                       ValidIssuer = Configuration["Authentication:Issuer"],

                       ValidateAudience = true,
                       ValidAudience = Configuration["Authentication:Audience"],

                       ValidateLifetime = true,

                       IssuerSigningKey = new SymmetricSecurityKey(secretByte)
                   };
               });

            services.AddControllers(setuAction=>
            {
                setuAction.ReturnHttpNotAcceptable = true;
                //setuAction.OutputFormatters.Add(
                //    new XmlDataContractSerializerOutputFormatter()
                //    );
            }).AddNewtonsoftJson(setupAction => {
                setupAction.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            }).AddXmlDataContractSerializerFormatters();
            services.AddTransient<ITouristRouteRepository, TouristRouteRepository>();
            //连接数据库
            services.AddDbContext<AppDbContext>(option=>
            {
                //option.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=FakeXcDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
                //option.UseSqlServer("server=localhost;Database=FakeXcDB;User Id=sa;Password=123456");
                option.UseSqlServer(Configuration["DbContext:ConnectionString"]);
            });

            //注册Swagger生成器，定义一个和多个Swagger 文档
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FakeXiecheng.API", Version = "v1" });

                // 为 Swagger JSON and UI设置xml文档注释路径
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
                var xmlPath = Path.Combine(basePath, "FakeXiecheng.API.xml");
                c.IncludeXmlComments(xmlPath);

                //启用swagger授权认证
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                //添加全局安全条件
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                         new OpenApiSecurityScheme{
                             Reference = new OpenApiReference {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "Bearer"}
                    },
                    new string[] { }
                    }
                });
                //显示自定义的Heard Token
                //c.OperationFilter<AuthTokenHeaderParameter>();
            });


            //扫描Profile文件
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FakeXiecheng.API v1"));
            }

            app.UseHttpsRedirection();

            // 你在哪？
            app.UseRouting();
            // 你是谁？
            app.UseAuthentication();
            // 你可以干什么？有什么权限？
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
