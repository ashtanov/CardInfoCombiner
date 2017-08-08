using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardInfoCombiner.ViewModels
{
    public class CurrentFolder : INotifyPropertyChanged
    {
        private string currentFolderPath;

        public string CFProperty
        {
            get { return currentFolderPath; }
            set
            {
                currentFolderPath = value;
                OnPropertyChanged("CFProperty");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
