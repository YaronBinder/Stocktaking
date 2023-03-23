using Stocktaking.ViewModel;
using System.Collections;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Stocktaking.Views;

/// <summary>
/// Interaction logic for ReportWindow.xaml
/// </summary>
public partial class ReportWindow : Window
{
    #region Constructor

    public ReportWindow()
    {
        InitializeComponent();
        reportWindowVM = new(this, DataGridTable);
        DataContext = reportWindowVM;
    }

    #endregion

    #region Variables

    private readonly ReportWindowVM reportWindowVM;

    #endregion

    #region Methods

    private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    private void DataGridTable_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        reportWindowVM.SelectedRows.Clear();
        IList items = DataGridTable.SelectedItems;
        foreach (var item in items)
            reportWindowVM.SelectedRows.Add(item as DataRowView);
    }

    #endregion
}
