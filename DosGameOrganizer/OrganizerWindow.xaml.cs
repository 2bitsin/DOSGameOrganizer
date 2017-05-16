using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        ObservableCollection<GameMetadataModel> m_DataList = new ObservableCollection<GameMetadataModel>();
        ICollectionView m_DataView = null;        

        public event PropertyChangedEventHandler PropertyChanged;

        string _StartupFile;
        public string SelectedStartupFile
        {
            set
            {
                if (_StartupFile != value)
                {
                    _StartupFile = value;
                    _U("CanRunInDOSBox");
                    Trace.TraceInformation("Selected Item : {0}", value);
                }
            }
        }

        bool _CanRunInDOSBox_IsEnabled = true;

        public bool CanRunInDOSBox_IsEnabled
        {
            get { return _CanRunInDOSBox_IsEnabled; }
            set
            {
                if (_CanRunInDOSBox_IsEnabled != value)
                {
                    _CanRunInDOSBox_IsEnabled = value;
                    _U("CanRunInDOSBox");
                }
            }
        }

        public bool CanRunInDOSBox
        {
            get
            {
                return _CanRunInDOSBox_IsEnabled
                    && Directory.Exists(Properties.Settings.Default.ExtractionPath)
                    && File.Exists(Properties.Settings.Default.DOSBoxPath)
                    && (_StartupFile?.Length > 0) ;
            }
        }

        public ICollectionView DataList
        {
            get
            {
                return m_DataView;
            }
        }

        string m_TextFilter = null;
        private void _U(string Name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        }
        public string TextFilter
        {
            set
            {
                m_TextFilter = value;
                _U("TextFilter");
                _U("DataList");
                m_DataView.Filter = (object _o) => { return FilterGrid(_o as GameMetadataModel); };
            }
            get
            {
                return m_TextFilter;
            }
        }
        
        private bool FilterGrid(GameMetadataModel _object)
        {            
            if (m_TextFilter == null || m_TextFilter.Length == 0)
            {
                return true;
            }

            var _key = m_TextFilter.ToLower();
            if (_object.Title.ToLower().Contains(_key)
             || _object.Developer.ToLower().Contains(_key)
             || _object.Path.ToLower().Contains(_key))
                return true;
            return false;
        }

        public OrganizerWindow()
        {
            InitializeComponent();
            DataContext = this;
            m_DataView = CollectionViewSource.GetDefaultView(m_DataList);

            if (Properties.Settings.Default.LastGameDir != "")
            {
                ScanDirectory(Properties.Settings.Default.LastGameDir);
            }

            Properties.Settings.Default.PropertyChanged += (object _sender, PropertyChangedEventArgs _event) =>
            {
                _U("CanRunInDOSBox");
            };
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
                Properties.Settings.Default.LastGameDir = _Dialog.SelectedPath;
                Properties.Settings.Default.Save();
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

            var _item = new GameMetadataModel()
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

        private void Settings_Save(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void Settings_Reset(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reload();
        }

        private void BrowseExtractionPath(object sender, RoutedEventArgs e)
        {
            var _Dialog = new FolderBrowserDialog()
            {
                Description = "Select folder to extract to.",
                ShowNewFolderButton = true,
                SelectedPath = "."                
            };

            if (_Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Properties.Settings.Default.ExtractionPath = _Dialog.SelectedPath;
            }
        }

        private void BrowseDOSBoxPath(object sender, RoutedEventArgs e)
        {
            var _Dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "DOS Box Executable(dosbox.exe)|DOSBox.exe",
                InitialDirectory = System.IO.Path.GetFullPath("."),
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (_Dialog.ShowDialog() == true)
            {
                Properties.Settings.Default.DOSBoxPath = _Dialog.FileName;
            }
        }

        private void RunInDosBox(object _sender, RoutedEventArgs _event)
        {
            CanRunInDOSBox_IsEnabled = false;
            _GlobalStatusText.Text = "Preparing to launch, please be patient ...";
            var _meta = _Grid.SelectedValue as GameMetadataModel;
            var _task = _meta.Run(this._ExecutableSelect.SelectedValue as String);
            _task.ContinueWith((Task _theTask) =>
            {
                CanRunInDOSBox_IsEnabled = true;
                this.Dispatcher.Invoke(() => _GlobalStatusText.Text = "Ready.");
            });
        }

        private void RevealInExplorer(object _sender, RoutedEventArgs _event)
        {
            if(_sender is FrameworkElement _element)
            {
                if (_element.DataContext is GameMetadataModel _meta)
                {
                    _meta.Explore();
                }
            }
        }

        private void ExtractGameData(object _sender, RoutedEventArgs _event)
        {
            if (_sender is FrameworkElement _element)
            {
                if (_element.DataContext is GameMetadataModel _meta)
                {
                    var _status = (Window.GetWindow(_element) as OrganizerWindow)._GlobalStatusText;
                    _status.Text = "Extracting ...";
                        
                    _meta.Extract().ContinueWith((_task) =>
                    {
                        _U("GameDataPath");
                        _U("GameDataPathExists");
                        this.Dispatcher.Invoke(() =>
                        {
                            _status.Text = "Complete.";
                        });
                    });
                }
            }
        }
        private void DeleteGameData(object _sender, RoutedEventArgs _event)
        {
            if (_sender is FrameworkElement _element)
            {
                if (_element.DataContext is GameMetadataModel _meta)
                {

                    if (Directory.Exists(_meta.GameDataPath))
                    {
                        _meta.DeleteGameData();
                        _U("GameDataPath");
                        _U("GameDataPathExists");
                    }
                }
            }
        }
    }
}
