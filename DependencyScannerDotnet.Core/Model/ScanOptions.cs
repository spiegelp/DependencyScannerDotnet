using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Model
{
    public class ScanOptions : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int m_maxScanDepth;
        private bool m_scanConflictsIntoDepth;

        public int MaxScanDepth
        {
            get
            {
                return m_maxScanDepth;
            }

            set
            {
                m_maxScanDepth = value;

                OnPropertyChanged();
            }
        }

        public bool ScanConflictsIntoDepth
        {
            get
            {
                return m_scanConflictsIntoDepth;
            }

            set
            {
                m_scanConflictsIntoDepth = value;

                OnPropertyChanged();
            }
        }

        public ScanOptions()
        {
            m_scanConflictsIntoDepth = true;
            m_maxScanDepth = 64;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null && !string.IsNullOrWhiteSpace(propertyName))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
