using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mobile.SignalR
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSignalRConnection("https://localhost:5001/touchless");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    internal static class Extensions
    {
        internal static IServiceCollection AddSignalRConnection(this IServiceCollection services, string hubUrl)
        {
            HubConnection connection = new HubConnectionBuilder()
               .WithUrl(hubUrl)
               .Build();

            connection.Closed += async (error) =>
            {
                Console.WriteLine("Reconnect");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            connection.On<string>("OnEvent", message => {
                // TODO: message processing here
                Console.WriteLine(message);
            });

            connection.StartAsync();

            services.AddSingleton<HubConnection>(connection);
            return services;
        }
    }
}
