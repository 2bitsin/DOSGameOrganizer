using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace DosGameOrganizer
{
    public class ZipFileModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        string _Path;
        ICollectionView _DirectoryEntriesView;
        List<ZipArchiveEntry> _DirectoryEntries;
        Task _EnumerateZipTask;
        String _StatusText;

        public Task ExtractTo(string _targetPath)
        {
            return Task.Run(() => ZipFile.ExtractToDirectory(_Path, _targetPath));
        }

        protected void _U(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public String StatusText
        {
            get { return _StatusText; }
            set
            {
                if (_StatusText != value)
                {
                    _StatusText = value;
                    _U(nameof(StatusText));
                }
            }
        }

        public ZipFileModel(string _path)
        {
            _Path = _path;
            _EnumerateZipTask = new Task(() =>
            {
                var _archive = ZipFile.OpenRead(_Path);
                var _entries = new List<ZipArchiveEntry>();

                foreach (var _entry in _archive.Entries)
                {
                    _entries.Add(_entry);
                }

                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    _DirectoryEntries = _entries;
                    _DirectoryEntriesView = CollectionViewSource.GetDefaultView(_entries);
                    _U(nameof(DirectoryEntries));
                });
            });
        }

        private bool FilterPredicate(object x)
        {
            if (_FilterExecutables)
            {
                var _ext = Path.GetExtension((x as ZipArchiveEntry).Name).ToLower();
                return _ext == ".exe" || _ext == ".com" || _ext == ".bat";
            }
            return true;
        }


        bool _FilterExecutables;
        public bool FilterExecutables
        {
            get { return _FilterExecutables; }
            set
            {
                if (_FilterExecutables != value)
                {
                    _FilterExecutables = value;

                    if (_DirectoryEntriesView != null)
                    {
                        _DirectoryEntriesView.Filter = FilterPredicate;
                    }
                    _U(nameof(FilterExecutables));
                    _U(nameof(DirectoryEntries));
                }
            }
        }

        public dynamic DirectoryEntries
        {
            get
            {
                if(_DirectoryEntriesView == null)
                {
                    _EnumerateZipTask.ContinueWith((_task) =>
                    {
                        Dispatcher.CurrentDispatcher.Invoke(() =>
                        {
                            FilterExecutables = true;
                        });
                    });
                    _EnumerateZipTask.Start();
                    return null;
                }
                return _DirectoryEntriesView;
            }
        }

    }
}
