using System;
using System.Threading.Tasks;
using System.Windows;
using ExplorerFM.Extensions;
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
                ? MongoProvider.TestConnection()
                : MySqlProvider.TestConnection();

            if (!string.IsNullOrWhiteSpace(testConnectionRes))
            {
                MessageBox.Show($"The following error prevents the application to start:\n{testConnectionRes}", "ExplorerFM - Error");
                Environment.Exit(0);
            }

            Task.Run(() => DataExtensions.FillCombination());

            new IntroWindow().ShowDialog();
        }
    }
}
