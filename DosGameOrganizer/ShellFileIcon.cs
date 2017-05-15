using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DosGameOrganizer
{
    class ShellFileIcon : System.Windows.Controls.Image
    {
        protected const uint SHGFI_ICON = 0x000000100;// get icon
        protected const uint SHGFI_DISPLAYNAME = 0x000000200;// get display name
        protected const uint SHGFI_TYPENAME = 0x000000400;// get type name
        protected const uint SHGFI_ATTRIBUTES = 0x000000800;// get attributes
        protected const uint SHGFI_ICONLOCATION = 0x000001000;// get icon location
        protected const uint SHGFI_EXETYPE = 0x000002000;// return exe type
        protected const uint SHGFI_SYSICONINDEX = 0x000004000;// get system icon index
        protected const uint SHGFI_LINKOVERLAY = 0x000008000;// put a link overlay on icon
        protected const uint SHGFI_SELECTED = 0x000010000;// show icon in selected state
        protected const uint SHGFI_ATTR_SPECIFIED = 0x000020000;// get only specified attributes
        protected const uint SHGFI_LARGEICON = 0x000000000;// get large icon
        protected const uint SHGFI_SMALLICON = 0x000000001;// get small icon
        protected const uint SHGFI_OPENICON = 0x000000002;// get open icon
        protected const uint SHGFI_SHELLICONSIZE = 0x000000004;// get shell size icon
        protected const uint SHGFI_PIDL = 0x000000008;// pszPath is a pidl
        protected const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;// use passed dwFileAttribute
        
        protected const uint FILE_ATTRIBUTE_READONLY = 0x00000001;
        protected const uint FILE_ATTRIBUTE_HIDDEN = 0x00000002;
        protected const uint FILE_ATTRIBUTE_SYSTEM = 0x00000004;
        protected const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        protected const uint FILE_ATTRIBUTE_ARCHIVE = 0x00000020;
        protected const uint FILE_ATTRIBUTE_DEVICE = 0x00000040;
        protected const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        protected const uint FILE_ATTRIBUTE_TEMPORARY = 0x00000100;
        protected const uint FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200;
        protected const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;
        protected const uint FILE_ATTRIBUTE_COMPRESSED = 0x00000800;
        protected const uint FILE_ATTRIBUTE_OFFLINE = 0x00001000;
        protected const uint FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000;
        protected const uint FILE_ATTRIBUTE_ENCRYPTED = 0x00004000;

        [StructLayout(LayoutKind.Sequential)]
        protected struct SHFILEINFO
        {
            public const int NAMESIZE = 80;
            public const int MAX_PATH = 260;
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NAMESIZE)]
            public string szTypeName;
        };

        [DllImport("shell32.dll")]
        protected static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("User32.dll")]
        protected static extern int DestroyIcon(System.IntPtr hIcon);

        public virtual string FilePath
        {
            get { return (string)this.GetValue(FilePathProperty); }
            set
            {
                if (FilePath != value)
                {
                    this.SetValue(FilePathProperty, value);
                    _UpdateSource();
                }
            }
        } 
        
        public ShellFileIcon()
        {
            Height = 16;
            Width = 16;
            _UpdateSource();
        }

        protected static Dictionary<Tuple<String, int>, System.Windows.Media.ImageSource> _Cache = new Dictionary<Tuple<string, int>, System.Windows.Media.ImageSource>();

        protected static System.Windows.Media.ImageSource LoadIcon(string _path, int _width, int _height)
        {
            _path = Path.GetExtension(_path).ToUpper();
            var _size = Math.Max(_width, _height);
            var _tkey = new Tuple<string, int>(_path, _size);

            if (_Cache.ContainsKey(_tkey))
            {
                return _Cache[_tkey];
            }

            var _shfi = new SHFILEINFO();
            var _flags = SHGFI_USEFILEATTRIBUTES + SHGFI_ICON;

            if (_size > 16)
            {
                _flags += SHGFI_LARGEICON;
            }
            else
            {
                _flags += SHGFI_SMALLICON;
            }

            var _stsz = (uint)System.Runtime.InteropServices.Marshal.SizeOf(_shfi);

            SHGetFileInfo(_path, FILE_ATTRIBUTE_NORMAL, ref _shfi, _stsz, _flags);
            var _source = Imaging.CreateBitmapSourceFromHIcon(_shfi.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            _Cache.Add(_tkey, _source);

            DestroyIcon(_shfi.hIcon);

            return _source;
        }

        protected void _UpdateSource()
        {
            this.Source = LoadIcon(FilePath, (int)Width, (int)Height);
        }

        protected static void _PropertyChanged(DependencyObject _sender, DependencyPropertyChangedEventArgs _event)
        {
            (_sender as ShellFileIcon)._UpdateSource();
        }

        static FrameworkPropertyMetadata FilePathMetadata = new FrameworkPropertyMetadata(@".txt", new PropertyChangedCallback(_PropertyChanged));

        static public DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(ShellFileIcon), FilePathMetadata);
    }
}
