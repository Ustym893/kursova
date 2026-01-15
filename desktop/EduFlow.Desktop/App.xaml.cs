using Microsoft.Extensions.DependencyInjection;

namespace EduFlow.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = default!;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        Services = services;

        MainPage = new NavigationPage(new EduFlow.Desktop.Views.RolePage());
    }
}