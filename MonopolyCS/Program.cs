using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using MonopolyCS.Configuration;
using NLog;

namespace MonopolyCS
{
    internal static class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureServices((hostContext, services) =>
                {
                    IMonopolyCsConfigMgr cm = new MonopolyCsConfigMgr();
                    hostContext.Configuration.Bind("Configurations", cm);
                    
                    GlobalDiagnosticsContext.Set("logDirectory", cm.EnvironmentVariables.LogDirectory);
                    
                    Logger.Info("Starting Monopoly service");
                    Logger.Trace($"----- SERVICE INFO -----");
                    Logger.Trace($"----- App Name:        {cm.EnvironmentVariables.AppName}");
                    Logger.Trace($"----- Production:      {hostContext.HostingEnvironment.IsProduction()}");
                    Logger.Trace($"----- Staging:         {hostContext.HostingEnvironment.IsStaging()}");
                    Logger.Trace($"----- Development:     {hostContext.HostingEnvironment.IsDevelopment()}");

                    services.AddSingleton(cm);
                    
                    services.AddHostedService<MonopolyWorker>();
                });
    }
}