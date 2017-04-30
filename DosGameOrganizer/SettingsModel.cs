using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DosGameOrganizer
{
    [Serializable]
    class SettingsModel : INotifyPropertyChanged
    {
        private void _Update(string _Property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_Property));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        String m_DOSBoxPath;
        String m_ExtractionPath;
        public String DOSBoxPath
        {
            get { return m_DOSBoxPath; }
            set { m_DOSBoxPath = value; _Update("DOSBoxPath"); }
        }
        public String ExtractionPath
        {
            get { return m_ExtractionPath; }
            set { m_ExtractionPath = value; _Update("ExtractionPath"); }
        }        
    }
}
