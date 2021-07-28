using ClassRunner4_6_1DotNet.common;
using MilBatDBModels.Common;
using Stocktaking.ViewModel.Base;
using Stocktaking.Views;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Trigger = Stocktaking.Models.Trigger;

namespace Stocktaking.ViewModel
{
    public class StocktakingVM : BaseVM, INotifyPropertyChanged
    {
        #region Constructor

        public StocktakingVM(BarcodeManager barcodeManager, Window window) : base(window)
        {
            _window = window;
            _barcodeManager = barcodeManager;
            _barcodeManager.BarcodeAction = BarcodeEvent;
        }

        #endregion

        #region Commands

        private ICommand _ReportWindow;
        public ICommand ReportWindow => _ReportWindow ??= new RelayCommand(OpenReportWindow);

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
                OnPropertyChanged(nameof(_Barcode));
            }
        }

        #endregion

        #region Variables

        // Main window reference
        private readonly Window _window;

        // Flag to check if the ReportWindow is already opened
        private bool _isReportWindowOpen = false;

        // Serial port barcode scanner manager
        private readonly BarcodeManager _barcodeManager;

        #endregion

        #region Properties

        public string BatteryNumber { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Performing communication with the 'SICK' camera and get the number of batteries in the tray
        /// Then save the data to SQL
        /// </summary>
        /// <param name="barcode"></param>
        public void BarcodeEvent(string barcode)
        {
            string countedTray = Trigger.PerformTrigger(_window.Close);
            BatteryNumber = countedTray;
            if (!ShowMode)
            {

            }
        }

        private void OpenReportWindow()
        {
            if (_isReportWindowOpen) return;
            _isReportWindowOpen = true;
            _window.WindowState = WindowState.Minimized;
            Thread reportWindowThread = new(new ThreadStart(new Action(() =>
                {
                    ReportWindow reportWindow = new();
                    reportWindow.Closed += delegate
                    {
                        _isReportWindowOpen = false;
                        Application.Current.Dispatcher?.Invoke(
                            new Action(() => 
                            { 
                                _window.WindowState = WindowState.Normal; 
                            }));
                        Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                    };
                    reportWindow.ShowDialog();
                    Dispatcher.Run();
                })));
            reportWindowThread.SetApartmentState(ApartmentState.STA);
            reportWindowThread.IsBackground = true;
            reportWindowThread.Start();
        }

        #endregion
    }

    //public class StocktakingVM : IniBase, INotifyPropertyChanged
    //{
    //    #region Constructor

    //    public StocktakingVM(BarcodeManager barcodeManager, Window window) : base(_savePropertiesPath)
    //    {
    //        _window = window;
    //        _barcodeManager = barcodeManager;
    //        _barcodeManager.BarcodeAction = BarcodeEvent;
    //        _LightCheckState = bool.Parse(GetString(_majorKey, _lightMinorKey, "true"));
    //        _ShowMode = bool.Parse(GetString(_majorKey, _modeMinorKey, "true"));
    //    }

    //    #endregion

    //    #region Variables

    //    // Closing action of the main app
    //    private readonly Window _window;

    //    // Major and Minor keys for the Inifile
    //    private readonly string _modeMinorKey = "Mode";
    //    private readonly string _lightMinorKey = "Light";
    //    private readonly string _majorKey = "Stocktaking";

    //    // The path to save the user preferences
    //    private static readonly string _savePropertiesPath = @"C:\pulser\GlobalConfig.ini";

    //    // Serial port barcode scanner manager
    //    private readonly BarcodeManager _barcodeManager;

    //    public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

    //    #endregion

    //    #region Properties

    //    public string BatteryNumber { get; private set; }

    //    #endregion

    //    #region Full Properties

    //    private string _Barcode;
    //    public string Barcode
    //    {
    //        get => _Barcode;
    //        set
    //        {
    //            _Barcode = value;
    //            BarcodeEvent(value.ToString());
    //        }
    //    }

    //    private bool _LightCheckState;
    //    public bool LightCheckState
    //    {
    //        get => _LightCheckState;
    //        set
    //        {
    //            _LightCheckState = value;
    //            WriteData(_majorKey, _lightMinorKey, value);
    //        }
    //    }

    //    private bool _ShowMode;
    //    public bool ShowMode 
    //    {
    //        get => _ShowMode;
    //        set
    //        {
    //            _ShowMode = value;
    //            WriteData(_majorKey, _modeMinorKey, value);
    //        }
    //    }

    //    #endregion

    //    #region Commands

    //    private ICommand _closeButton;
    //    public ICommand CloseButton => _closeButton ??= new RelayCommand(_window.Close);

    //    private ICommand _minimizeButton;
    //    public ICommand MinimizeButton => _minimizeButton ??= new RelayCommand(() => _window.WindowState = WindowState.Minimized);

    //    #endregion

    //    #region Methods

    //    /// <summary>
    //    /// Performing communication  with the 'SICK' camera and get the number of batteries in the tray
    //    /// Then save the data to SQL
    //    /// </summary>
    //    /// <param name="barcode"></param>
    //    private void BarcodeEvent(string barcode)
    //    {
    //        string countedTray = Trigger.PerformTrigger(_window.Close);
    //        BatteryNumber = countedTray;
    //        if (!_ShowMode)
    //        {

    //        }
    //    }

    //    /// <summary>
    //    /// Fire the <see cref="PropertyChanged"/> event
    //    /// </summary>
    //    /// <param name="name">The property actual name</param>
    //    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    //    #endregion
    //}
}