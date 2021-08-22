using System;
using System.Windows;
using ExplorerFM.Windows;

namespace ExplorerFM
{
    public partial class App : Application
    {
        private static Random _randomizer = null;

        public static Random Randomizer => _randomizer ?? (_randomizer = new Random());

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            new IntroWindow().ShowDialog();
        }
    }
}
