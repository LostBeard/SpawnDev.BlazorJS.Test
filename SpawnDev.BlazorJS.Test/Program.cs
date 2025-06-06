using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.Diagnostics;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.JsonConverters;
using SpawnDev.BlazorJS.Test;
using SpawnDev.BlazorJS.Test.Services;
using SpawnDev.BlazorJS.Test.Shared;
using SpawnDev.BlazorJS.Test.UnitTests;
using SpawnDev.BlazorJS.Toolbox;
using SpawnDev.BlazorJS.WebWorkers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

HybridObjectConverterFactory.Verbose = false;
// Add SpawnDev.BlazorJS.BlazorJSRuntime
builder.Services.AddBlazorJSRuntime(out var JS);
// Add SpawnDev.BlazorJS.WebWorkers.WebWorkerService
builder.Services.AddWebWorkerService(WebWorkerService =>
{
    WebWorkerService.TaskPool.MaxPoolSize = WebWorkerService.MaxWorkerCount;
    //WebWorkerService.TaskPool.PoolSize = WebWorkerService.MaxWorkerCount;
});

// Add services
builder.Services.AddScoped((sp) => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddKeyedSingleton<ITestService2>("apples", (_, key) => new TestService2((string)key!));
builder.Services.AddKeyedSingleton<ITestService2>("bananas", (_, key) => new TestService2((string)key!));

// The below service is used to test CallDispatcher used with WebWorkers (Used in UnitTests)
builder.Services.AddSingleton<AsyncCallDispatcherTest>();
// More app specific services
builder.Services.AddSingleton(builder.Configuration); // used to demo appsettings reading in workers
builder.Services.AddSingleton<MediaDevices>();
builder.Services.AddSingleton<MediaDevicesService>();
// Add app services that will be called on the main thread and/or worker threads
builder.Services.AddSingleton<IFaceAPIService, FaceAPIService>();
builder.Services.AddSingleton<IMathsService, MathsService>();

// Radzen UI services
builder.Services.AddSingleton<DialogService>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddSingleton<TooltipService>();
builder.Services.AddSingleton<ContextMenuService>();

//builder.Services.AddSingleton<JSObjectAnalyzer>();

builder.Services.AddSingleton<CryptoService>();

builder.Services.AddSingleton<FileSystemAPIService>();
//
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

#if DEBUG && true
var host = builder.Build();
await host.StartBackgroundServices();
//
//if (JS.IsWindow)
//{
//    //var JS = host.Services.GetRequiredService<BlazorJSRuntime>();
//    var WebWorkerService = host.Services.GetRequiredService<WebWorkerService>();
//    var thisScope = JS.GlobalThisTypeName;
//    var calledScope = await WebWorkerService.TaskPool.Run(() => TestClass.TestIt(null, thisScope));
//    JS.Log($"This scope: {thisScope}. Calling scope: {calledScope}");
//}

await host.BlazorJSRunAsync();
#else
// build and Init using BlazorJSRunAsync (instead of RunAsync)
await builder.Build().BlazorJSRunAsync();
#endif

static class TestClass
{
    public static async Task<string> TestIt([FromServices] BlazorJSRuntime js, string callingScope)
    {
        var thisScope = js.GlobalThisTypeName;
        js.Log($"This scope: {thisScope}. Calling scope: {callingScope}");
        return thisScope;
    }
}