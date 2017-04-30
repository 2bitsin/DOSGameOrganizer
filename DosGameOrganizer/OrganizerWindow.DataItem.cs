using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.IO.Compression;


namespace DosGameOrganizer
{
    public partial class OrganizerWindow : Window
    {
        public class DataItem: INotifyPropertyChanged
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

            public event PropertyChangedEventHandler PropertyChanged;
        };
    }
}
