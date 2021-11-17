using System.Windows;
using Stocktaking.ViewModel;
using ClassRunner4_6_1DotNet.common;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System;
using System.Threading;

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
        BarcodeManager barcodeManager = new();
        DataContext = new StocktakingVM(barcodeManager, this, HistoryPanel);
        Closed += barcodeManager.Close;
        HistoryBtn.Click += OpenClose_Click;
    }

    private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();

    private void OpenClose_Click(object sender, RoutedEventArgs e)
    {
        Storyboard sb = (isHistoryPanelOpen ? Resources["CloseMenu"] : Resources["OpenMenu"]) as Storyboard;
        sb.Begin(LeftMenu);
        isHistoryPanelOpen = !isHistoryPanelOpen;
    }
}
