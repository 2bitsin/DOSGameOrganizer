using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.IO.Compression;
using System.IO;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DosGameOrganizer
{
    [Serializable]
    public class GameMetadataModel : INotifyPropertyChanged
    {
        protected void _U(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        ZipFileModel _Archive;
        public ZipFileModel Archive => _Archive;        

        string _Developer;
        public string GameDataPath
        {
            get
            {
                return $"{Properties.Settings.Default.ExtractionPath}\\{System.IO.Path.GetFileNameWithoutExtension(_Path)}";
            }
        }

        public string Developer
        {
            get { return _Developer; }
            set
            {
                if (_Developer != value)
                {
                    _Developer  = value;
                    _U("Developer");
                }
            }
        }
        
        string _Title;                                                                  
        public string Title
        {
            get { return _Title; }
            set
            {
                if (_Title != value)
                {
                    _Title = value;
                    _U("Title");
                }
            }
        }

        ulong _Year;
        public ulong  Year
        {
            get { return _Year; }
            set
            {
                if (_Year != value)
                {
                    _Year = value;
                    _U("Year");
                }
            }
        }

        string _Path;
        public string Path
        {
            get { return _Path; }
            set
            {
                if (_Path != value)
                {
                    if (_Path != null)
                    {

                        var _oldPath = GameDataPath;

                        File.Move(_Path, value);
                        _Path = value;
                        if (Directory.Exists(_oldPath))
                        {
                            Directory.Move(_oldPath, GameDataPath);
                        }
                    }
                    else
                    {
                        _Path = value;
                    }
                    _Archive = new ZipFileModel(_Path);

                    _U("Path");
                    _U("Archive");
                    _U("GameDataPath");
                    _U("GameDataPathExists");                    
                }
            }
        }

        public void DeleteGameData()
        {
            Directory.Delete(GameDataPath, true);
            _U("GameDataPathExists");
        }

        public bool GameDataPathExists
        {
            get
            {
                return Directory.Exists(GameDataPath);
            }
        }

        public long FileSize { get { return (new FileInfo(Path).Length); } }
        public String FileSizeAsString
        {
            get
            {
                var _size = FileSize;
                var _base = (int)Math.Log(_size, 1024);
                var _powr = Math.Pow(1024, _base);
                var _sufx = (new String[] { "", "KB", "MB", "GB", "TB" })[_base];
                return Math.Round(_size / _powr, 2) + " " + _sufx;
            }
        }

        public Task Extract()
        {
            var _archive = this.Archive;
            if (!Directory.Exists(GameDataPath))
            {
                Directory.CreateDirectory(GameDataPath);                
                return _archive.ExtractTo(GameDataPath).ContinueWith((task) =>
                {
                    _U("GameDataPath");
                    _U("GameDataPathExists");
                });
            }
            return Task.Run(() => { });
        }

        public Task Run(string _executable)
        {
            
            return Task.Run(() =>
            {
                Extract().Wait();

                var _confDir = System.IO.Path.GetDirectoryName(Properties.Settings.Default.DOSBoxPath) + "\\DOSBox.conf";
                var _process = Process.Start(Properties.Settings.Default.DOSBoxPath, $"\"{GameDataPath}\\{_executable}\" -conf \"{_confDir}\"");
            });
        }

        public void Explore()
        {
            if (Directory.Exists(GameDataPath))
            {
                var _path = $"\"{GameDataPath}\"";
                Process.Start("explorer.exe", _path);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    };
    
}
