using System.Windows;
using Stocktaking.ViewModel;
using ClassRunner4_6_1DotNet.common;

namespace Stocktaking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BarcodeManager barcodeManager = new();
            DataContext = new StocktakingVM(barcodeManager, Close);
            Closed += delegate
            {
                barcodeManager.Close();
            };
        }
    }
}
