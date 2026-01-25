using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace Server
{
    public class WebProgram
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Use Kestrel and listen on port 8005
            builder.WebHost.UseKestrel();
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(8005);
            });

            // Add hosted service that runs the TCP server
            builder.Services.AddHostedService<ServerHostedService>();

            var app = builder.Build();

            // Serve static files from wwwroot (default web root)
            // Ensure index.html is used as the default document
            var defaultFilesOptions = new DefaultFilesOptions();
            defaultFilesOptions.DefaultFileNames.Clear();
            defaultFilesOptions.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(defaultFilesOptions);
            app.UseStaticFiles();

            // For compatibility with older client paths that reference /Client/*, map /Client to the webroot as well
            var webroot = app.Environment.WebRootPath;
            if (!string.IsNullOrEmpty(webroot) && Directory.Exists(webroot))
            {
                var webRootProvider = new PhysicalFileProvider(webroot);
                app.UseStaticFiles(new StaticFileOptions { FileProvider = webRootProvider, RequestPath = "/Client" });
            }

            // Enable WebSocket support and map /chat to the in-process WebSocket handler
            app.UseWebSockets();
            app.Map("/chat", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {   
                    var ws = await context.WebSockets.AcceptWebSocketAsync();
                    await Server.HandleWebSocket(ws);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            });

            app.Run();
        }
    }
}
