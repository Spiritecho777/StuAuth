using StuAuth.Properties;
using StuAuth.Classe;
using System.Configuration;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;

namespace StuAuth
{
    public partial class App : Application
    {
        private static Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool isNewInstance;
            var loc2 = (Loc)Application.Current.Resources["Loc"];
            mutex = new Mutex(true, "YourUniqueMutexName", out isNewInstance);

            if (!isNewInstance)
            {
                MessageBox.Show(loc2["App1"], "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown(); // Ferme l'application si une autre instance est déjà en cours
                return;
            }

            base.OnStartup(e);

            string savedLang = Settings.Default.LangCode;

            var loc = (Loc)Application.Current.Resources["Loc"];
            loc.Culture = new CultureInfo(savedLang);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex.ReleaseMutex(); // Libère le mutex à la fermeture de l'application
            base.OnExit(e);
        }

        #region Manipulation faite suite a la fermeture - crash du logiciel
        public App()
        {
            this.Exit += OnAppExit; // Appelée lorsque l'application se termine proprement.
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit; // ProcessExit appelé même si l'application est tuée.
        }

        // Méthode appelée lorsque l'application se termine proprement
        private void OnAppExit(object sender, ExitEventArgs e)
        {
            DeleteFile();
        }

        // Méthode appelée lorsque le processus est tué ou termine
        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            DeleteFile();
        }

        private void DeleteFile()
        {
            string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");
            string filePath = System.IO.Path.Combine(appDirectory, "Account_decrypted.dat");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            string filePath2 = System.IO.Path.Combine(appDirectory, "Account.dat");

            if (File.Exists(filePath2))
            {
                long fileSize = new FileInfo(filePath2).Length;

                if (fileSize == 0 && File.Exists(filePath2))
                {
                    File.Delete(filePath2);
                }
            }
        }
        #endregion
    }
}