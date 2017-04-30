using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DosGameOrganizer
{
    /// <summary>
    /// Interaction logic for OrganizerWindow.xaml
    /// </summary>
    public partial class OrganizerWindow : Window
    {

        Window m_Window;
        ObservableCollection<DataItem> m_DataList = new ObservableCollection<DataItem>();
        public ObservableCollection<DataItem> DataList => m_DataList;

        public OrganizerWindow()
        {
            InitializeComponent();
            DataContext = this;
            ScanDirectory(@"E:\Games\DOS");
        }
        private void OpenPreviewClick(object _sender, RoutedEventArgs _event)
        {
            m_Window = new Win32IconResourcePreviewWindow();
            m_Window.Show();
        }

        private void ScanDirectoryClick(object _sender, RoutedEventArgs _events)
        {
            var _Dialog = new FolderBrowserDialog()
            {
                Description = "Select folder to scan.",
                ShowNewFolderButton = false,
                SelectedPath = "."
            };
            
            if (_Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ScanDirectory(_Dialog.SelectedPath);
            }                
        }
        private void ScanDirectory(string _Path)
        {
            foreach (var _entry in Directory.GetFiles(_Path))
            {
                ProcessFile(_entry);
            }
            foreach (var _entry in Directory.GetDirectories(_Path))
            {
                ScanDirectory(_entry);
            }
        }
        private bool ProcessFile(string _entry)
        {
            var _fullPath = _entry;
            if (System.IO.Path.GetExtension(_entry) != ".zip")
            {
                return false;
            }
            _entry = System.IO.Path.GetFileName(_entry);

            var _Matches = _Regex_ParseFileName.Match(_entry);

            if (!_Matches.Success)
            {
                Console.WriteLine("Unable to parse file name: {0}.", _entry);
                return false;
            }

            var _Title  = _Matches.Groups[1]?.Value;
            var _Second = _Matches.Groups[2]?.Value;
            var _Third  = _Matches.Groups[3]?.Value;


            var _Year = new ulong();
            var _Developer = "";

            if (ulong.TryParse(_Second, out _Year))
            {
                _Developer = _Third;
            }
            else if (ulong.TryParse(_Third, out _Year))
            {
                _Developer = _Second;
            }
            else
            {
                Console.WriteLine("Unable to parse file name: {0}.", _entry);
                return false;
            }

            var _item = new DataItem()
            {
                Path       = _fullPath,
                Title      = _Title.Trim(),
                Year       = _Year,
                Developer  = _Developer.Trim()
            };

            m_DataList.Add(_item);
            return true;
        }
        private void ExitFromApp(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
        
        static Regex _Regex_ParseFileName = new Regex(@"([^\(]+)\(([^\)]+)\)\(([^\)]+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private void _Grid_LoadingRow(object _sender, DataGridRowEventArgs _event)
        {
            _event.Row.Header = (_event.Row.GetIndex()).ToString();
        }

        private void _Grid_SelectionChanged(object _sender, SelectionChangedEventArgs _event)
        {

        }
    }
}
