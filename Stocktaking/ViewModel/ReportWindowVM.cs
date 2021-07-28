using Stocktaking.ViewModel.Base;
using System.ComponentModel;
using System.Windows;

namespace Stocktaking.ViewModel
{
    public class ReportWindowVM : BaseVM, INotifyPropertyChanged
    {
        #region Constructor

        public ReportWindowVM(Window window) : base(window)
        {
            _window = window;
        }

        #endregion

        #region Variables

        private readonly Window _window;

        #endregion

        #region Full Properties

        private string _CartNumber;
        public string CartNumber
        {
            get => _CartNumber;
            set
            {
                _CartNumber = value;
                OnPropertyChanged(nameof(CartNumber));
            }
        }

        private string _TrayNumber;
        public string TrayNumber
        {
            get => _TrayNumber; 
            set 
            {
                _TrayNumber = value;
                OnPropertyChanged(nameof(TrayNumber));
            }
        }

        private string _DateFrom;
        public string DateFrom
        {
            get => _DateFrom; 
            set 
            {
                _DateFrom = value;
                OnPropertyChanged(nameof(DateFrom));
            }
        }

        private string _DateTo;
        public string DateTo
        {
            get => _DateTo;
            set
            {
                _DateTo = value;
                OnPropertyChanged(nameof(DateTo));
            }
        }

        private string _ExecutionTime;
        public string ExecutionTime
        {
            get => _ExecutionTime; 
            set 
            {
                _ExecutionTime = value;
                OnPropertyChanged(nameof(ExecutionTime));
            }
        }

        #endregion
    }
}
