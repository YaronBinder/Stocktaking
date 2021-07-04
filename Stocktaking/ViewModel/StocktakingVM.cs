using System;
using IniFile;
using PropertyChanged;
using Stocktaking.Models;
using System.ComponentModel;
using ClassRunner4_6_1DotNet.common;

namespace Stocktaking.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class StocktakingVM : IniBase, INotifyPropertyChanged
    {
        #region Constructor

        public StocktakingVM(BarcodeManager barcodeManager, Action close) : base(path)
        {
            this.close = close;
            this.barcodeManager = barcodeManager;
            this.barcodeManager.BarcodeAction = BarcodeEvent;
            _LightCheckState = bool.Parse(GetString(majorKey, lightMinorKey, "true"));
            _ShowMode = bool.Parse(GetString(majorKey, modeMinorKey, "true"));
        }

        #endregion

        #region Variables

        private readonly Action close;

        private readonly string modeMinorKey = "Mode";
        private readonly string lightMinorKey = "Light";
        private readonly string majorKey = "Stocktaking";

        private static readonly string path = @"C:\pulser\GlobalConfig.ini";

        private readonly BarcodeManager barcodeManager;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public string BatteryNumber { get; private set; }

        #endregion

        #region Full Properties

        private string _Barcode;
        public string Barcode
        {
            get => _Barcode;
            set
            {
                _Barcode = value;
                BarcodeEvent(value.ToString());
            }
        }

        private bool _LightCheckState;
        public bool LightCheckState
        {
            get => _LightCheckState;
            set
            {
                _LightCheckState = value;
                WriteData(majorKey, lightMinorKey, value);
            }
        }

        private bool _ShowMode;
        public bool ShowMode 
        {
            get => _ShowMode;
            set
            {
                _ShowMode = value;
                WriteData(majorKey, modeMinorKey, value);
            }
        }

        #endregion

        #region Methods

        private void BarcodeEvent(string barcode)
        {
            string countedTray = Trigger.PerformTrigger("192.168.0.1", 2120, "\x02trigger\x03", close);
            BatteryNumber = countedTray;
            if (!_ShowMode)
            {

            }
        }

        #endregion
    }
}