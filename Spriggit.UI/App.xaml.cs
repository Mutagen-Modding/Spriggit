using System.IO;
using System.Windows;
using Autofac;
using CommandLine;
using Microsoft.VisualBasic.Logging;
using Noggog.IO;
using Serilog;
using Spriggit.CLI;
using Spriggit.CLI.Commands;
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

        if (RunCommandLineIfAppropriate(e))
        {
            Application.Current.Shutdown();
            return;
        }

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

    private static bool RunCommandLineIfAppropriate(StartupEventArgs e)
    {
        var isCommandLine = new Parser((s) =>
            {
                s.IgnoreUnknownArguments = true;
            }).ParseArguments(e.Args, typeof(DeserializeCommand), typeof(SerializeCommand))
            .MapResult(
                (DeserializeCommand _) => true,
                (SerializeCommand _) => true,
                _ => false);

        if (isCommandLine)
        {
            Task.Run(() => Parser.Default.ParseArguments(e.Args, typeof(DeserializeCommand), typeof(SerializeCommand))
                .MapResult(
                    async (DeserializeCommand deserialize) => await Runner.Run(deserialize),
                    async (SerializeCommand serialize) => await Runner.Run(serialize),
                    async _ => -1)).Wait();
            return true;
        }

        return false;
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

        var curLog = Path.Combine("logs", "Current.log");
        if (File.Exists(curLog))
        {
            File.Delete(curLog);
        }

        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine("logs", logFileName))
            .WriteTo.File(curLog)
            .CreateLogger();

        return Serilog.Log.Logger;
    }
}