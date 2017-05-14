using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DosGameOrganizer
{
    public class Win32ResIcon : System.Windows.Controls.Image
    {
        public Win32ResIcon() : base()
        {
            _UpdateSource();   
        }

        public static readonly DependencyProperty ResourcePathProperty   = DependencyProperty.Register("ResourcePath",   typeof(String),  typeof(Win32ResIcon), new UIPropertyMetadata("shell32.dll",  new PropertyChangedCallback(_PropertyChanged)));
        public static readonly DependencyProperty ResourceIndexProperty  = DependencyProperty.Register("ResourceIndex",  typeof(Int32),   typeof(Win32ResIcon), new UIPropertyMetadata(0,              new PropertyChangedCallback(_PropertyChanged)));
        public static readonly DependencyProperty LargeIconProperty      = DependencyProperty.Register("LargeIcon",      typeof(Boolean), typeof(Win32ResIcon), new UIPropertyMetadata(true,           new PropertyChangedCallback(_PropertyChanged)));

        public virtual String ResourcePath
        {
            get
            {
                return (String)this.GetValue(ResourcePathProperty);
            }
            set
            {                
                this.SetValue(ResourcePathProperty, value);
            }
        }
        public virtual Int32 ResourceIndex
        {
            get
            {
                return (Int32)this.GetValue(ResourceIndexProperty);
            }
            set
            {
                this.SetValue(ResourceIndexProperty, value);
            }
        }
        public virtual Boolean LargeIcon
        {
            get
            {
                return (Boolean)this.GetValue(LargeIconProperty);
            }
            set
            {
                this.SetValue(LargeIconProperty, value);
            }
        }

        protected virtual void _UpdateSource()
        {
            IntPtr _SmallIcon, _LargeIcon;
            if (ExtractIconEx(ResourcePath, ResourceIndex, out _SmallIcon, out _LargeIcon, 1) > 0)
            {
                var _TargetHIcon = LargeIcon ? _LargeIcon : _SmallIcon;
                this.Source = Imaging.CreateBitmapSourceFromHIcon(_TargetHIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DestroyIcon(_SmallIcon);
                DestroyIcon(_LargeIcon);
            }
        }

        protected static void _PropertyChanged(DependencyObject _object, DependencyPropertyChangedEventArgs _event)
        {
            if (_event.Property != ResourceIndexProperty
             && _event.Property != ResourcePathProperty
             && _event.Property != LargeIconProperty)
            {
                return;
            }

            (_object as Win32ResIcon)._UpdateSource();
        }

        public static int GetNumberOfIcons(String _Path)
        {            
            return ExtractIconEx_NoRef(_Path, -1, IntPtr.Zero, IntPtr.Zero, 0);
        }

        [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

        [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int ExtractIconEx_NoRef(string sFile, int iIndex, IntPtr piLargeVersion, IntPtr piSmallVersion, int amountIcons);

        [DllImport("User32.dll", EntryPoint = "DestroyIcon", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int DestroyIcon(IntPtr _Handle);

    }
}
