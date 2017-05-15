using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.IO.Compression;
using System.IO;

namespace DosGameOrganizer
{
    [Serializable]
    public class GameMetadataModel: INotifyPropertyChanged
    {
        protected void _U(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        string _Developer;    
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
        public string Path {
            get { return _Path; }
            set
            {
                if (_Path != value)
                {
                    _Path = value;
                    _U("Path");
                    _Archive = new ZipFileModel(_Path);
                    _U("Archive");
                }
            }
        }

        ZipFileModel _Archive;
        public ZipFileModel Archive => _Archive;        

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
    
        public event PropertyChangedEventHandler PropertyChanged;
    };
    
}
