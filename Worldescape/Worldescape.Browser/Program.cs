using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Worldescape.Service;

namespace Worldescape.Browser
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpService(baseAddress: new Uri(builder.HostEnvironment.BaseAddress));

            var host = builder.Build();
            await host.RunAsync();
        }

        public static void RunApplication()
        {
            Application.RunApplication(() =>
            {
                var app = new Worldescape.App();
            });
        }
    }
}
