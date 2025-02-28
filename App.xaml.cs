using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;
using System.Diagnostics;

namespace MFA
{
    public partial class App : Application
    {
        private static Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool isNewInstance;
            mutex = new Mutex(true, "YourUniqueMutexName", out isNewInstance);

            if (!isNewInstance)
            {
                MessageBox.Show("L'application est déjà en cours d'exécution.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown(); // Ferme l'application si une autre instance est déjà en cours
                return;
            }

            base.OnStartup(e);
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