using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ClassRunner.common;
using Stocktaking.ViewModel;

namespace Stocktaking;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class StocktakingWindow : Window
{
    private bool isHistoryPanelOpen = false;

    public StocktakingWindow()
    {
        InitializeComponent();
        ScannerManager barcodeManager = new();
        DataContext = new StocktakingVM(barcodeManager, this, HistoryPanel);
        Closed += barcodeManager.Close;
    }

    private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

    private void OpenClose_Click(object sender, RoutedEventArgs e)
    {
        Storyboard sb = (isHistoryPanelOpen ? Resources["CloseMenu"] : Resources["OpenMenu"]) as Storyboard;
        sb.Begin(LeftMenu);
        isHistoryPanelOpen = !isHistoryPanelOpen;
    }
}
