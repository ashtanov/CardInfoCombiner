using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardInfoCombiner.ViewModels
{
    public class CardsReady : INotifyPropertyChanged
    {
        private int currentCardsReady;

        public int CardReadyProperty
        {
            get { return currentCardsReady; }
            set
            {
                currentCardsReady = value;
                OnPropertyChanged("CardReadyProperty");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
