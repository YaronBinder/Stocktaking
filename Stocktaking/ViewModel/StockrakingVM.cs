using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using PropertyChanged;
using System.Threading.Tasks;
using ClassRunner4_6_1DotNet.common;

namespace Stocktaking.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class StockrakingVM : INotifyPropertyChanged
    {
        #region Constructor

        public StockrakingVM()
        {
            barcodeManager = new();
        }

        #endregion
        
        #region Properties

        private bool _IsToggleChecked;
        public bool IsToggleChecked
        {
            get => _IsToggleChecked;
            set
            {
                _IsToggleChecked = value;
                OnPropertyChanged(nameof(IsToggleChecked));
            }
        }

        private int _BatteryNumber = 255;
        public int BatteryNumber
        {
            get => _BatteryNumber;
            set
            {
                _BatteryNumber = value;
                OnPropertyChanged(nameof(BatteryNumber));
            }
        }

        private int _Barcode;
        public int Barcode
        {
            get => _Barcode; 
            set 
            {
                _Barcode = value;
                OnPropertyChanged(nameof(Barcode));
            }
        }

        #endregion

        #region Variables

        private BarcodeManager barcodeManager;

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => PropertyChangedEvent(s, e);

        #endregion

        #region Methods

        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private static void PropertyChangedEvent(object sender, PropertyChangedEventArgs e) { }

        #endregion
    }
}
