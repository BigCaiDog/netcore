using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using NLog.Extensions.Logging;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.IO;


namespace hrcore.Admin {
	public class Startup {
		public Startup(IHostingEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);

            if (incSys.isDubug)
                builder = builder.AddJsonFile("appsettingsDebug.json", true, true);
            else
                builder = builder.AddJsonFile("appsettingsRelease.json", true, true);

            //if (env.IsProduction())
            //	builder.AddJsonFile("/var/webos/hrcore/appsettings.json", true, true);

            this.Configuration = builder.AddEnvironmentVariables().Build();
			this.env = env;

			Newtonsoft.Json.JsonConvert.DefaultSettings = () => {
				var st = new Newtonsoft.Json.JsonSerializerSettings();
				st.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
				return st;
			};
		}

		public IConfigurationRoot Configuration { get; }
		public IHostingEnvironment env { get; }

		public void ConfigureServices(IServiceCollection services) {
			services.AddSingleton<IDistributedCache>(new RedisCache());
			services.AddSession(a => {
				a.IdleTimeout = TimeSpan.FromMinutes(30);
				a.CookieName = "Session_hrcore";
			}).AddMvc();

            //#region Swagger UI
            //if (env.IsDevelopment())
            //    services.AddSwaggerGen(options =>
            //    {
            //        options.SwaggerDoc("v1", new Info
            //        {
            //            Version = "v1",
            //            Title = "hrcore API",
            //            Description = "hrcore 项目webapi接口说明",
            //            TermsOfService = "None",
            //            Contact = new Contact { Name = "duoyi", Email = "", Url = "http://duoyi.com" },
            //            License = new License { Name = "duoyi", Url = "http://duoyi.com" }
            //        });
            //        options.IgnoreObsoleteActions();
            //        //options.IgnoreObsoleteControllers(); // 类、方法标记 [Obsolete]，可以阻止【Swagger文档】生成
            //        options.DescribeAllEnumsAsStrings();
            //        //options.IncludeXmlComments(AppContext.BaseDirectory + @"/Admin.xml"); // 使用前需开启项目注释 xmldoc
            //        options.OperationFilter<FormDataOperationFilter>();
            //    });
            //#endregion
            services.AddSingleton<IConfigurationRoot>(Configuration);
			services.AddSingleton<IHostingEnvironment>(env);
            services.AddDataProtection();
            //配置https
            services.Configure<Microsoft.AspNetCore.Server.Kestrel.KestrelServerOptions>(option => { option.UseHttps(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "hitcaidogpfx.pfx"), "123qwe"); });

		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			Console.OutputEncoding = Encoding.GetEncoding("GB2312");
			Console.InputEncoding = Encoding.GetEncoding("GB2312");

            // 以下写日志会严重影响吞吐量，高并发项目建议改成 redis 订阅发布形式
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddNLog().AddDebug();

            env.ConfigureNLog("nlog.config");


            if (incSys.isDubug)
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("view/error");

            hrcore.BLL.RedisHelper.InitializeConfiguration(Configuration);
			hrcore.DAL.SqlHelper.Instance.Log = loggerFactory.CreateLogger("hrcore_DAL_sqlhelper");

			app.UseSession().UseMvc();
            app.UseCors(builder => builder.WithOrigins("https://*").AllowAnyHeader());
			app.UseDefaultFiles().UseStaticFiles(); //UseDefaultFiles 必须在 UseStaticFiles 之前调用

            //if (env.IsDevelopment())
            //    app.UseSwagger().UseSwaggerUi(options =>
            //    {
            //        options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");
            //    });
        }
	}
}
