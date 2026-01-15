using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
namespace EduFlow.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = default!;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        Services = services;

        MainPage = new NavigationPage(new EduFlow.Desktop.Views.RolePage());
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try
            {
                File.WriteAllText(
                    Path.Combine(FileSystem.AppDataDirectory, "eduflow-crash.txt"),
                    e.ExceptionObject?.ToString() ?? "Unknown unhandled exception"
                );
            }
            catch { }
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            try
            {
                File.WriteAllText(
                    Path.Combine(FileSystem.AppDataDirectory, "eduflow-crash.txt"),
                    e.Exception.ToString()
                );
            }
            catch { }
        };
        
    }
}