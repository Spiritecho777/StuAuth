using System.Configuration;
using System.Data;
using System.Windows;

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
    }

}
