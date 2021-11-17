using Stocktaking.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
