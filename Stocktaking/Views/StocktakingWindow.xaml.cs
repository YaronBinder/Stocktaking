using System.Windows;
using Stocktaking.ViewModel;
using ClassRunner4_6_1DotNet.common;
using System.Windows.Input;

namespace Stocktaking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StocktakingWindow : Window
    {
        public StocktakingWindow()
        {
            InitializeComponent();
            BarcodeManager barcodeManager = new();
            DataContext = new StocktakingVM(barcodeManager, this);
            Closed += barcodeManager.Close;
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
