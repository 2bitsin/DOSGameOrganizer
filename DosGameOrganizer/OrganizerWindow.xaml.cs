using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class OrganizerWindow : Window, INotifyPropertyChanged
    {        
        Window m_Window;
        ObservableCollection<GridDataModel> m_DataList = new ObservableCollection<GridDataModel>();
        ICollectionView m_DataView = null;        

        public event PropertyChangedEventHandler PropertyChanged;

        public ICollectionView DataList
        {
            get
            {
                return m_DataView;
            }
        }

        string m_TextFilter = null;
        public string TextFilter
        {
            set
            {
                m_TextFilter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextFilter"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DataList"));
                m_DataView.Filter = (object _o) => { return FilterGrid(_o as GridDataModel); };
            }
            get
            {
                return m_TextFilter;
            }
        }
        
        private bool FilterGrid(GridDataModel _object)
        {            
            if (m_TextFilter == null || m_TextFilter.Length == 0)
            {
                return true;
            }

            if (_object.Title.Contains(m_TextFilter)
             || _object.Developer.Contains(m_TextFilter)
             || _object.Path.Contains(m_TextFilter))
                return true;
            return false;
        }

        public OrganizerWindow()
        {
            InitializeComponent();
            DataContext = this;
            m_DataView = CollectionViewSource.GetDefaultView(m_DataList);
            ScanDirectory(@"E:\Games\DOS");
        }
        private void OpenPreviewClick(object _sender, RoutedEventArgs _event)
        {
            m_Window = new Win32IconResourcePreviewWindow();
            m_Window.Show();
        }

        private void _Toobar_ScanDirectoryClick(object _sender, RoutedEventArgs _events)
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

            var _item = new GridDataModel()
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

        private void _Toolbar_ClearSearch(object _sender, RoutedEventArgs _event)
        {
            TextFilter = "";
        }
    }
}
