using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace StuAuth.Classe
{
    public class HttpServer
    {
        private Main mainPage;

        private CancellationTokenSource cts;
        private bool isServerRunning = false;

        public HttpServer(Main mainPage)
        {
            this.mainPage = mainPage;
        }
        
        public void Start(string ip)
        {
            cts = new CancellationTokenSource();

            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls($"http://{ip}:19755/");

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy.AllowAnyOrigin()
                                    .AllowAnyMethod()
                                    .AllowAnyHeader());
            });

            var app = builder.Build();

            app.UseCors("AllowAll");

            app.MapGet("/", async context =>
            {
                var responseObject = new
                {
                    Accounts = GetAccounts(),
                    Folder = GetFolder()
                };
                string jsonResponse = JsonSerializer.Serialize(responseObject);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(jsonResponse);
            });

            isServerRunning = true;
            _ = app.RunAsync(cts.Token);
        }

        private string GetAccounts()
        {
            string appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
            string filepath = Path.Combine(appDirectory, "Account_decrypted.dat");

            if (File.Exists(filepath))
            {
                string[] lines = File.ReadAllLines(filepath);
                for (int i = 0; i < lines.Length; i++)
                {
                    int index = lines[i].IndexOf(";");
                    if (index != -1)
                    {
                        lines[i] = lines[i].Substring(index + 1).Trim();
                    }
                }
                return string.Join("\n", lines);
            }
            else
            {
                return "Pas de compte trouver";
            }
        }

        private string GetFolder()
        {
            string appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
            string filepath = Path.Combine(appDirectory, "Account_decrypted.dat");

            if (File.Exists(filepath))
            {
                string[] lines = File.ReadAllLines(filepath);
                for (int i = 0; i < lines.Length; i++)
                {
                    int index = lines[i].IndexOf("\\");
                    if (index != -1)
                    {
                        lines[i] = lines[i].Substring(0, index).Trim();
                    }
                }
                return string.Join("\n", lines);
            }
            else
            {
                return "Pas de compte trouver";
            }
        }

        public void Stop()
        {
            Console.WriteLine("Arrêt en cours...");
            Task.Delay(1000).Wait();
            cts.Cancel();
        }
    }
}
