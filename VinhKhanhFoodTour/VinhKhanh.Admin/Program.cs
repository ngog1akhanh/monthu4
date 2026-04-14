using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using VinhKhanh.Admin;
using MudBlazor.Services; // Thêm ở trên cùng

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();


// ... code của bạn
builder.Services.AddMudServices(); // Thêm dòng này để nạp CSS/JS của MudBlazor
                                   // ...