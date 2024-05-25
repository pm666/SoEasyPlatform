using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SqlSugar.IOC;
namespace SoEasyPlatform
{
    /// <summary>
    /// �������
    /// </summary>
    public class Startup
    {
        #region ���ò���
        /// <summary>
        /// �汾��
        /// </summary>
        public static string Version = "1.5.7";
        /// <summary>
        /// �ӿ�����Ŀ¼
        /// </summary>
        /// <param name="configuration"></param>
        public static string RootUrl = "/api/";
        #endregion

        #region ���÷���
        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        /// <summary>
        /// ���ö���
        /// </summary>
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Services.AddServices(services);
            services.AddSqlSugar(new SqlSugar.IOC.IocConfig()
            {
                ConfigId = "master1",
                DbType = IocDbType.Sqlite,
                IsAutoCloseConnection = true,
                ConnectionString = "DataSource=" + GetCurrentDirectory() + @"\database\sqlite.db"
            });
            services.ConfigurationSugar(db =>
            {
                if (!db.ConfigQuery.Any())
                {
                    db.ConfigQuery.SetTable<Template>(it => it.Id, it => it.Title);
                    db.ConfigQuery.SetTable<TemplateType>(it => it.Id, it => it.Name);
                }
            });
            Task.Run(() =>
            {
                #if !DEBUG
                Task.Run(() =>
                {
                    System.Threading.Thread.Sleep(200);
                    var url = "http://localhost:5000?id=" + SqlSugar.SnowFlakeSingle.Instance.NextId();
                    try
                    {
                        Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName =url, Arguments = "chrome.exe" });
                    }
                    catch (Exception)
                    {
                        Process.Start("explorer", url);
                    }
                });
               #endif
            });
        }

        public static string GetCurrentDirectory()
        {
#if DEBUG
            return AppContext.BaseDirectory;
#endif
            return System.Environment.CurrentDirectory;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Configures.AddConfigure(app, env);
        }
#endregion
    }
}
