using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting; // 1. KHAI BÁO THƯ VIỆN HỌA SĨ

namespace VinhKhanhFoodTour.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp() // 2. BẬT CÔNG TẮC ĐỂ VẼ BẢN ĐỒ
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }
}