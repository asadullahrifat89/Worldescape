using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Worldescape.Browser
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddTransient(sp => new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
                Timeout = new TimeSpan(1, 0, 0),
            });            

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
