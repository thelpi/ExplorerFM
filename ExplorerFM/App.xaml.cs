using System;
using System.Windows;
using ExplorerFM.Properties;
using ExplorerFM.Windows;

namespace ExplorerFM
{
    public partial class App : Application
    {
        private static Random _randomizer = null;

        public static Random Randomizer => _randomizer ?? (_randomizer = new Random());

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var testConnectionRes = MongoService.TestConnection(Settings.Default.MongoConnectionString, Settings.Default.MongoDatabase);

            if (!string.IsNullOrWhiteSpace(testConnectionRes))
            {
                MessageBox.Show($"The following error prevents the application to start:\n{testConnectionRes}", "ExplorerFM - Error");
                Environment.Exit(0);
            }

            new IntroWindow().ShowDialog();
        }
    }
}
