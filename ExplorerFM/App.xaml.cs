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
            var testConnectionRes = MySqlService.TestConnection(Settings.Default.ConnectionString);

            if (!string.IsNullOrWhiteSpace(testConnectionRes))
            {
                MessageBox.Show($"The following error prevents the application to start:" +
                        $"\n{testConnectionRes}" +
                        $"\n\nMake sure to have WAMP installed on your computer" +
                        $", then mount the database script provided with the source code.",
                    "ExplorerFM - Error");
                Environment.Exit(0);
            }

            new IntroWindow().ShowDialog();
        }
    }
}
