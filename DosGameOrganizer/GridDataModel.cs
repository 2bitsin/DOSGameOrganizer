using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.IO.Compression;
using System.IO;

namespace DosGameOrganizer
{
    public class GridDataModel: INotifyPropertyChanged
    {
        string _Path;
        string _Developer;
        string _Title;
        ulong _Year;
        List<String> _ZipContents;
        public string Path      { get { return _Path;      } set { _Path       = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Path")); }}
        public string Developer { get { return _Developer; } set { _Developer  = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Developer")); } }
        public string Title     { get { return _Title;     } set { _Title      = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Title")); } }
        public ulong  Year      { get { return _Year;      } set { _Year       = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Year")); } }
        public List<String> ZipContents
        {
            get
            {
                if (_ZipContents == null)
                {
                    var _entries = new List<String>();
                    using (var _archive = ZipFile.OpenRead(Path))
                    {
                        foreach (var _entry in _archive.Entries)
                        {
                            _entries.Add(_entry.FullName.ToUpper());
                        }
                    }
                    _ZipContents = _entries;
                }
                return _ZipContents;
            }
        }
    
        public long FileSize
        {
            get
            {
                return (new FileInfo(Path).Length);
            }
        }
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
