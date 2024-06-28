using OtpNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StuAuth.Classe
{
    public class HttpServer
    {
        private HttpListener listener;
        private Thread listenerThread;
        private Main mainPage;
        private CancellationTokenSource cancellationTokenSource;

        public HttpServer(Main mainPage)
        {
            this.mainPage = mainPage;
        }

        public void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:19755/");
            cancellationTokenSource = new CancellationTokenSource();
            listenerThread = new Thread(() => StartListening(cancellationTokenSource.Token));
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        private void StartListening(CancellationToken cancellationToken)
        {
            listener.Start();
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (mainPage.isServerRunning)
                    {
                        var context = listener.GetContext();
                        ProcessRequest(context);
                    }
                }
            }
            catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
            {
                // Listener was stopped, exit the loop.
            }
            finally
            {
                listener.Close();
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            string responseString = string.Empty;
            //System.Diagnostics.Debug.WriteLine(context.Request.Url.AbsolutePath);
            if (context.Request.Url.AbsolutePath == "/")
            {
                responseString = GetAccounts();
            }
            else { responseString = "Requete Invalide"; }

            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        private string GetAccounts()
        {
            string appDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
            string filepath = Path.Combine(appDirectory, "Account.dat");

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

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            listener.Stop();
            listenerThread.Join();
        }
    }
}
