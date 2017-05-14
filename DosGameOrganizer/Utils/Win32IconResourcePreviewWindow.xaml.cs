using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DosGameOrganizer
{
    /// <summary>
    /// Interaction logic for Win32IconResourcePreviewWindow.xaml
    /// </summary>
    public partial class Win32IconResourcePreviewWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private String m_Path = @"shell32.dll";
        public Win32IconResourcePreviewWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadIconsFrom(m_Path);
        }

        public String Path
        {
            get { return m_Path; }
            set
            {
                m_Path = value;
                LoadIconsFrom(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Path"));
            }
        }

        private static List<String> m_SeatchPath = new List<String>()
        {
            Environment.SystemDirectory,
            Environment.GetEnvironmentVariable("SystemRoot")          
        };

        private String LocateFile(string _Path)
        {
            if (!File.Exists(_Path))
            {
                bool _Found = false;

                foreach (var _item in m_SeatchPath)
                {
                    if (File.Exists(_item + @"\" + _Path))
                    {
                        _Found = true;
                        _Path = _item + @"\" + _Path;
                        break;
                    }
                }

                if (!_Found)
                {
                    return null;
                }
            }
            return _Path;
        }

        public bool LoadIconsFrom(string _Path)
        {
            _Path = LocateFile(_Path);

            if (_Path == null)
            {
                return false;
            }
            this.m_WrapPanel.Children.Clear();
            var n = Win32ResIcon.GetNumberOfIcons(_Path);
            for (var i = 0; i < n; ++i)
            {
                var item = new Win32ResIcon()
                {
                    ResourcePath = _Path,
                    ResourceIndex = i,
                    LargeIcon = true,
                    Height = 32,
                    Width = 32,
                };

                item.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.Fant);
                item.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
                item.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                item.SetValue(MarginProperty, new Thickness(5));
                item.SetValue(ToolTipProperty, string.Format("{0}@{1}", _Path, i));

                this.m_WrapPanel.Children.Add(item);
            }
            return true;
        }

        private void BrowseForIcons(object _sender, RoutedEventArgs _event)
        {
            var _Dialog = new OpenFileDialog();
            _Dialog.InitialDirectory = Environment.SystemDirectory;
            _Dialog.CheckFileExists = true;
            _Dialog.Filter = @"Application filles|*.dll;*.exe|Icon files|*.ico|All files|*.*";
            if (_Dialog.ShowDialog() == true)
            {
                Path = _Dialog.FileName;
            }
        }
    }
}
