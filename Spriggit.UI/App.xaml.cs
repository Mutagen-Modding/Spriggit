using System.IO;
using System.Windows;
using Autofac;
using Microsoft.VisualBasic.Logging;
using Noggog.IO;
using Serilog;
using Spriggit.UI.Services;

namespace Spriggit.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
            
        var singleApp = new SingletonApplicationEnforcer("Spriggit");

        if (!singleApp.IsFirstApplication)
        {
            singleApp.ForwardArgs(e.Args);
            Application.Current.Shutdown();
            return;
        }

        var logger = GetLogger();

        var window = new MainWindow();
            
        try
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<Spriggit.UI.Modules.MainModule>();
            builder.RegisterInstance(window)
                .AsSelf()
                .As<IMainWindow>();
            builder.RegisterInstance(logger).As<ILogger>();
            var container = builder.Build();

            container.Resolve<Startup>()
                .Initialize();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error constructing container");
            throw;
        }
            
        window.Show();
    }

    private static ILogger GetLogger()
    {
        var startDt = DateTime.Now;
        var startTime = $"{startDt:HH_mm_ss}";
        startTime = startTime.Remove(5, 1);
        startTime = startTime.Remove(2, 1);
        startTime = startTime.Insert(2, "h");
        startTime = startTime.Insert(5, "m");
        startTime += "s";
        var logFileName = $"{startDt:MM-dd-yyyy}_{startTime}.log";

        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine("logs", logFileName))
            .CreateLogger();

        return Serilog.Log.Logger;
    }
}