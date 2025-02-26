using System;
using System.Windows;
using ExplorerFM.Properties;
using ExplorerFM.Providers;
using ExplorerFM.Windows;

namespace ExplorerFM
{
    public partial class App : Application
    {
        private static Random _randomizer = null;

        public static Random Randomizer => _randomizer ?? (_randomizer = new Random());

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var testConnectionRes = Settings.Default.DataProvider == nameof(MongoProvider)
                ? MongoProvider.TestConnection(Settings.Default.MongoConnectionString, Settings.Default.MongoDatabase)
                : MySqlProvider.TestConnection(Settings.Default.MySqlConnectionString);

            if (!string.IsNullOrWhiteSpace(testConnectionRes))
            {
                MessageBox.Show($"The following error prevents the application to start:\n{testConnectionRes}", "ExplorerFM - Error");
                Environment.Exit(0);
            }

            new IntroWindow().ShowDialog();
        }
    }
}
