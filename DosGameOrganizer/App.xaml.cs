using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DosGameOrganizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Init(object _sender, StartupEventArgs _event)
        {
            MainWindow = new OrganizerWindow();
            MainWindow.Show();
        }
    }
}
