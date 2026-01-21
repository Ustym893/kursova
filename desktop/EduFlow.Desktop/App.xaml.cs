using EduFlow.Desktop.Services;
using EduFlow.Desktop.Views;

namespace EduFlow.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = default!;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        Services = services;

        // Temporary page
        MainPage = new ContentPage
        {
            Content = new VerticalStackLayout
            {
                Padding = 32,
                Children =
                {
                    new Label
                    {
                        Text = "EduFlow",
                        FontSize = 24,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new ActivityIndicator
                    {
                        IsRunning = true
                    }
                }
            }
        };
    }

    protected override async void OnStart()
    {
        base.OnStart();

        var auth = Services.GetRequiredService<IAuthStore>();
        await auth.LoadAsync();

        // 🔴 CRITICAL: switch page on UI thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!string.IsNullOrWhiteSpace(auth.Token) &&
                !string.IsNullOrWhiteSpace(auth.Role))
            {
                if (auth.Role == "TEACHER")
                    MainPage = new NavigationPage(new TeacherHomePage());
                else
                    MainPage = new NavigationPage(new StudentHomePage());
            }
            else
            {
                MainPage = new NavigationPage(new RolePage());
            }
        });
    }
}