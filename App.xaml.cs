using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BetterVideoPlayer
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public string[] arguments;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            arguments = e.Args;
        }
    }
}
