using IniFile;
using MilBatDBModels.Common;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Stocktaking.ViewModel.Base
{
    public class BaseVM : IniBase, INotifyPropertyChanged
    {
        #region Constructor

        public BaseVM(Window window) : base(_iniFilePath)
        {
            _window = window;
            _LightCheckState = bool.Parse(GetString(_majorKey, _lightMinorKey, "true"));
            _ShowMode = bool.Parse(GetString(_majorKey, _modeMinorKey, "true"));
        }

        #endregion

        #region Variables

        // Major and Minor keys for the ini file
        private readonly string _modeMinorKey = "Mode";
        private readonly string _lightMinorKey = "Light";
        private readonly string _majorKey = "Stocktaking";

        // The path to save the user preferences
        private static readonly string _iniFilePath = @"C:\pulser\GlobalConfig.ini";

        // The relevent sended view window
        private readonly Window _window;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Full Properties

        private bool _LightCheckState;
        public bool LightCheckState
        {
            get => _LightCheckState;
            set
            {
                _LightCheckState = value;
                WriteData(_majorKey, _lightMinorKey, value);
                OnPropertyChanged(nameof(LightCheckState));
            }
        }

        private bool _ShowMode;
        public bool ShowMode
        {
            get => _ShowMode;
            set
            {
                _ShowMode = value;
                WriteData(_majorKey, _modeMinorKey, value);
                OnPropertyChanged(nameof(ShowMode));
            }
        }

        #endregion

        #region Commands

        private ICommand _CloseButton;
        public ICommand CloseButton => _CloseButton ??= new RelayCommand(_window.Close);

        private ICommand _MinimizeButton;
        public ICommand MinimizeButton => _MinimizeButton ??= new RelayCommand(() => _window.WindowState = WindowState.Minimized);

        #endregion

        #region Methods

        /// <summary>
        /// Fire the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="name">The property actual name</param>
        public void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}
