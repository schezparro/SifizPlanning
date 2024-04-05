using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using Microsoft.Owin;
using Microsoft.AspNet.SignalR;
using Hangfire;
using Hangfire.SqlServer;
using System.Diagnostics;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;

namespace SifizPlanning
{
    public class Startup
    {
        private IEnumerable<IDisposable> GetHangfireServers()
        {
            string connString = ConfigurationManager.ConnectionStrings["SifizPlanningEntidades"].ConnectionString;
            string providerConnectionString = new EntityConnectionStringBuilder(connString).ProviderConnectionString;
            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(providerConnectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });
            yield return new BackgroundJobServer();
        }
        public void Configuration(IAppBuilder app)
        {
            System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)(768 | 3072);
            var hubConfiguration = new HubConfiguration();
            hubConfiguration.EnableDetailedErrors = true;
            app.MapSignalR(hubConfiguration);
            //app.UseHangfireAspNet(GetHangfireServers);

            //if (System.Configuration.ConfigurationManager.AppSettings["hangfire"] == "true")
            //    app.UseHangfireDashboard();
            //Hangfire hangfire = new Hangfire();
            //hangfire.Start();
        }
    }
}