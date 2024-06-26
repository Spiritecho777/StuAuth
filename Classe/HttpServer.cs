using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StuAuth.Classe
{
    public class HttpServer
    {
        private HttpListener listener;
        private Thread listenerThread;
        private MainWindow mainPage;

        public HttpServer(MainWindow mainPage)
        {
            this.mainPage = mainPage;
        }

        public void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:19755/");
            listenerThread = new Thread(new ThreadStart(StartListening));
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        private void StartListening()
        {
            listener.Start();
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                ProcessRequest(context);
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
                for (int i = 0; i < lines.Length;i++)
                {
                    int index = lines[i].IndexOf(";");
                    if (index != -1)
                    {
                        lines[i] = lines[i].Substring(index+1).Trim();
                    }
                }
                return string.Join("\n",lines);
            }
            else
            {
                return "Pas de compte trouver";
            }
        }

        public void Stop()
        {
            listener.Stop();
            listenerThread.Abort();
        }
    }
}
