using Microsoft.Extensions.Logging;

namespace EduFlow.Desktop;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
		builder.Services.AddSingleton<EduFlow.Desktop.Services.IAuthStore, EduFlow.Desktop.Services.AuthStore>();
		builder.Services.AddSingleton<EduFlow.Desktop.Services.ApiClient>();
		builder.Services.AddSingleton<App>();
		return builder.Build();
	}
}
