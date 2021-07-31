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
            //�������ݿ�
            services.AddDbContext<AppDbContext>(option=>
            {
                //option.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=FakeXcDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
                //option.UseSqlServer("server=localhost;Database=FakeXcDB;User Id=sa;Password=123456");
                option.UseSqlServer(Configuration["DbContext:ConnectionString"]);
            });

            //ע��Swagger������������һ���Ͷ��Swagger �ĵ�
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FakeXiecheng.API", Version = "v1" });

                // Ϊ Swagger JSON and UI����xml�ĵ�ע��·��
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//��ȡӦ�ó�������Ŀ¼�����ԣ����ܹ���Ŀ¼Ӱ�죬������ô˷�����ȡ·����
                var xmlPath = Path.Combine(basePath, "FakeXiecheng.API.xml");
                c.IncludeXmlComments(xmlPath);

                //����swagger��Ȩ��֤
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "���¿�����������ͷ����Ҫ���Jwt��ȨToken��Bearer Token",
                    Name = "Authorization",//jwtĬ�ϵĲ�������
                    In = ParameterLocation.Header,//jwtĬ�ϴ��authorization��Ϣ��λ��(����ͷ��)
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                //���ȫ�ְ�ȫ����
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
                //��ʾ�Զ����Heard Token
                //c.OperationFilter<AuthTokenHeaderParameter>();
            });


            //ɨ��Profile�ļ�
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

            // �����ģ�
            app.UseRouting();
            // ����˭��
            app.UseAuthentication();
            // ����Ը�ʲô����ʲôȨ�ޣ�
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
