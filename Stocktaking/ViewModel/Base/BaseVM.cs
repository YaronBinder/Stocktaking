using IniFile;
using MilBatDBModels.Common;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Configuration;
using AppDep = System.Deployment.Application.ApplicationDeployment;

namespace Stocktaking.ViewModel.Base;

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
    private const string _modeMinorKey = "Mode";
    private const string _lightMinorKey = "Light";
    private const string _majorKey = "Stocktaking";

    // The path to save the user preferences
    private const string _iniFilePath = @"C:\pulser\GlobalConfig.ini";

    // The relevent sended view window
    private readonly Window _window;

    // SQL connection string
    public readonly static string connectionString = ConfigurationManager.ConnectionStrings["StockingTable"].ConnectionString;

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region Properties

    public string Title => $"Stocktaking v{GetCurrentVersion()}";

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

    private static string GetCurrentVersion()
        => AppDep.IsNetworkDeployed
        ? AppDep.CurrentDeployment.CurrentVersion.ToString(4)
        : System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4);

    #endregion
}
