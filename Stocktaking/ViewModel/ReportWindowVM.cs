using System;
using System.Data;
using System.Text;
using System.Linq;
using CommonWindows;
using System.Drawing;
using System.Threading;
using System.Reflection;
using Stocktaking.Models;
using System.ComponentModel;
using System.Data.SqlClient;
using MilBatDBModels.Common;
using Stocktaking.ViewModel.Base;
using System.Collections.Generic;
using Window = System.Windows.Window;
using DataTable = System.Data.DataTable;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Threading;
using Windows;
using Windows.ModelView;

namespace Stocktaking.ViewModel;

public class ReportWindowVM : BaseVM, INotifyPropertyChanged
{
    #region Constructor

    public ReportWindowVM(Window window, DataGrid dataGridTable) : base(window)
    {
        TableFiller();
        _dataGrid = dataGridTable;
    }

    #endregion

    #region Properties

    private readonly DataGrid _dataGrid;

    public List<DataRowView> SelectedRows { get; set; } = new();

    #endregion

    #region Full Properties

    private DataTable _Table = new();
    public DataTable Table
    {
        get => _Table;
        set
        {
            _Table = value;
            OnPropertyChanged(nameof(Table));
        }
    }

    private string _CellType;
    public string CellType
    {
        get => _CellType;
        set
        {
            _CellType = value;
            TableFilter();
            OnPropertyChanged(nameof(CellType));
        }
    }

    private string _CartNumber;
    public string CartNumber
    {
        get => _CartNumber;
        set
        {
            _CartNumber = value;
            TableFilter();
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
            TableFilter();
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
            TableFilter();
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
            TableFilter();
            OnPropertyChanged(nameof(DateTo));
        }
    }

    private string _ExecutionTimeFrom;
    public string ExecutionTimeFrom
    {
        get => _ExecutionTimeFrom;
        set
        {
            _ExecutionTimeFrom = value;
            TableFilter();
            OnPropertyChanged(nameof(ExecutionTimeFrom));
        }
    }

    private string _ExecutionTimeTo;
    public string ExecutionTimeTo
    {
        get => _ExecutionTimeTo;
        set
        {
            _ExecutionTimeTo = value;
            TableFilter();
            OnPropertyChanged(nameof(ExecutionTimeTo));
        }
    }

    #endregion

    #region Commands

    private RelayCommand _ExcelReport;
    public RelayCommand ExcelReport => _ExcelReport ??= new RelayCommand(() => Task.Run(CreateExcelReport));

    private RelayCommand _RefreshTable;
    public RelayCommand RefreshTable => _RefreshTable ??= new RelayCommand(TableFiller);


    private RelayCommand _DeleteRow;
    public RelayCommand DeleteRow => _DeleteRow ??= new RelayCommand(DeleteRowAction);

    #endregion

    #region Methods

    /// <summary>
    /// Erase selected rows from StocktakingData
    /// </summary>
    private void DeleteRowAction()
    {
        if(SelectedRows.Count == 0)
        {
            new InfoBox("אישור", "יש לבחור רשומה/ות למחיקה", MessageLevel.OK).ShowDialog();
            return;
        }
        string verificationMessage = SelectedRows.Count > 1 ? $"האם למחוק {SelectedRows.Count} רשומות אלו?" : "האם למחוק רשומה זו?";
            YesNoWindow Verification = new(verificationMessage);
            Verification.ShowDialog();
            if (!Verification.ResultYes) return;

        int effectedRows = default;

        //Thread thread = new(() =>
        //{
            LoginUser loginUser = new(new() { });
            ((LoginUserMV)loginUser.DataContext).OnSuccessLogin += () =>
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();
                SelectedRows.ForEach(row =>
                {
                    string query = @"DELETE FROM [StocktakingData]
                                 WHERE [TrayBarcode] = @TrayBarcode
                                 AND [CartNumber] = @CartNumber
                                 AND [Date] = @Date";
                    using SqlTransaction transaction = connection.BeginTransaction();
                    using SqlCommand command = new(query, connection, transaction);
                    try
                    {
                        command.Parameters.AddWithValue("@TrayBarcode", row.Row["TrayBarcode"]);
                        command.Parameters.AddWithValue("@CartNumber", row.Row["CartNumber"]);
                        command.Parameters.AddWithValue("@Date", row.Row["Date"]);
                        effectedRows += command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                    }
                });
                connection.Close();
                TableFiller();
                new InfoBox("אישור", $"{effectedRows} שדות נמחקו בהצלחה", MessageLevel.OK).ShowDialog();
            };
            loginUser.ShowDialog();
        //});
        //thread.SetApartmentState(ApartmentState.STA);
        //thread.Start();
    }

    /// <summary>
    /// Create excel sheet report using the selected rows or the entire <see cref="Table"/>
    /// </summary>
    private void CreateExcelReport()
    {
        Excel.Range range;
        Excel._Workbook workbook;
        Excel._Worksheet worksheet;
        Excel.Application excelApp;

        try
        {
            bool resultYes = true;
            if (SelectedRows.Count <= 1)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    YesNoWindow yesNoWindow = new("האם ברצונך להוציא דו\"ח של כל הטבלה?");
                    yesNoWindow.ShowDialog();
                    resultYes = yesNoWindow.ResultYes;
                });
            }
            if (!resultYes) return;

            excelApp = new();
            if (excelApp is null)
            {
                new InfoBox("אישור", "יש להתקין EXCEL במשב זה", MessageLevel.Error).ShowDialog();
                return;
            }

            // Stacktaking table columns name
            string[] columns = new string[] { "CellType", "CartNumber", "TrayBarcode", "CellCount", "Date", "TimeStamp" };

            excelApp.Visible = true;
            workbook = excelApp.Workbooks.Add(Missing.Value);
            worksheet = workbook.ActiveSheet;

            // Caption for the spreadsheet
            worksheet.Cells[1, 1] = "דוח ספירת מלאי";

            // Style for the caption
            range = worksheet.get_Range("A1", "F1");
            range.Merge();
            range.Font.Size = 32;
            range.Font.Bold = true;
            range.EntireColumn.AutoFit();
            range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;
            range.Interior.Color = ColorTranslator.FromHtml("#52b69a");
            range.BorderAround2(
                Excel.XlLineStyle.xlContinuous,
                Excel.XlBorderWeight.xlThick,
                Excel.XlColorIndex.xlColorIndexAutomatic,
                Excel.XlRgbColor.rgbBlack);

            // Caption of each column
            worksheet.Cells[2, 1] = "מספר עגלה";
            worksheet.Cells[2, 2] = "ברקוד מגש";
            worksheet.Cells[2, 3] = "מספר תאים במגש";
            worksheet.Cells[2, 4] = "תאריך מילוי";
            worksheet.Cells[2, 5] = "שעת מילוי";
            worksheet.Cells[2, 6] = "סכום התאים בעגלה";

            // Style for the columns
            range = worksheet.get_Range("A2", "F2");
            range.Font.Size = 22;
            range.EntireColumn.AutoFit();
            range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;
            range.Interior.Color = ColorTranslator.FromHtml("#cebeff");

            // Cell border style
            range.Cells.Borders.Color = Excel.XlRgbColor.rgbBlack;
            range.Cells.Borders.Weight = Excel.XlBorderWeight.xlMedium;
            range.Cells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            range.Cells.Borders.ColorIndex = Excel.XlColorIndex.xlColorIndexAutomatic;

            // Set dataRows to SelectedRows if more then one row is selected else Table rows
            List<DataRowView> dataRows = SelectedRows.Count > 1 ? SelectedRows : Table.DefaultView.Cast<DataRowView>().ToList();

            // The first cart number to sum it's cells number
            string cartNumber = dataRows[0]["CartNumber"].ToString();

            // Summary of the cells in each cart
            int sumOfCells = 0;
            int cellIndex = 3;

            // Iterate over dataRows and set data to the excel spreadsheet
            for (int i = 0; i < dataRows.Count; i++)
            {
                for (int j = 0; j < columns.Length; j++)
                {
                    if ((dataRows[i]["CartNumber"].ToString() != cartNumber || i == 0) && columns[j] == "CellType")
                    {
                        // Caption for the cell type
                        worksheet.Cells[cellIndex, 1] = dataRows[i][columns[j]];

                        // Style for the caption
                        range = worksheet.get_Range($"A{cellIndex}", $"F{cellIndex}");
                        range.Merge();
                        range.Font.Size = 16;
                        range.EntireColumn.AutoFit();
                        range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                        range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;
                        range.Cells.Borders.ColorIndex = Excel.XlColorIndex.xlColorIndexAutomatic;
                        range.Interior.Color = ColorTranslator.FromHtml("#d4a276");
                        range.BorderAround2(
                            Excel.XlLineStyle.xlContinuous,
                            Excel.XlBorderWeight.xlThin,
                            Excel.XlColorIndex.xlColorIndexAutomatic,
                            Excel.XlRgbColor.rgbBlack);
                        cellIndex++;
                        j++;
                    }
                    if (columns[j] == "CartNumber")
                    {
                        if (dataRows[i][columns[j]].ToString() == cartNumber)
                        {
                            sumOfCells += int.Parse(dataRows[i]["CellCount"].ToString());
                        }
                        else
                        {
                            // Assign the new cart number to the variable
                            cartNumber = dataRows[i]["CartNumber"].ToString();

                            // Write the summary of the cells in the last column of the last related row of the same cart
                            worksheet.Cells[cellIndex - 2, 6] = sumOfCells;

                            // Set the color of the summary cell to a different backgroung color
                            worksheet.get_Range($"F{cellIndex - 2}").Interior.Color = Excel.XlRgbColor.rgbIndianRed;

                            // Assign the number of cells from the first tray from different cart
                            sumOfCells = int.Parse(dataRows[i]["CellCount"].ToString());

                            // Create thick line between every cart
                            worksheet.get_Range($"A{cellIndex - 2}", $"F{cellIndex - 2}").Cells.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
                            worksheet.get_Range($"A{cellIndex - 2}", $"F{cellIndex - 2}").Cells.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = Excel.XlBorderWeight.xlThick;
                        }
                    }
                    if (j > 0)
                    {
                        worksheet.Cells[cellIndex, j] = dataRows[i][columns[j]];
                    }
                }
                // Style
                range = worksheet.get_Range($"A{cellIndex}", $"F{cellIndex}");
                range.Font.Size = 16;
                range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;

                // Border style
                range.Cells.Borders.Color = Excel.XlRgbColor.rgbBlack;
                range.Cells.Borders.Weight = Excel.XlBorderWeight.xlThin;
                range.Cells.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                range.Cells.Borders.ColorIndex = Excel.XlColorIndex.xlColorIndexAutomatic;

                cellIndex++;
            }
            cellIndex--;
            // Write the summary of the cells in the last column of the last related row of the same cart
            worksheet.Cells[cellIndex, 6] = sumOfCells;

            // Set the color of the summary cell to a different backgroung color
            worksheet.get_Range($"F{cellIndex}").Interior.Color = Excel.XlRgbColor.rgbIndianRed;

            // Create thick line between every cart
            worksheet.get_Range($"A{cellIndex}", $"F{cellIndex}").Cells.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = Excel.XlLineStyle.xlContinuous;
            worksheet.get_Range($"A{cellIndex}", $"F{cellIndex}").Cells.Borders[Excel.XlBordersIndex.xlEdgeBottom].Weight = Excel.XlBorderWeight.xlThick;

            excelApp.Visible = true;
            excelApp.UserControl = true;
        }
        catch (Exception e)
        {
            Application.Current?.Dispatcher.Invoke(new InfoBox("אישור", e.Message, MessageLevel.Error).ShowDialog);
            throw e;
        }
        SelectedRows.Clear();
    }

    /// <summary>
    /// Get the whole Stocktaking table 
    /// </summary>
    /// <returns>The whole Stocktaking table</returns>
    private void TableFiller()
    {
        PleaseWait pleaseWait = new();
        pleaseWait.Show();
        string query = @"SELECT * FROM StocktakingData";
        using SqlConnection connection = new(connectionString);
        using SqlCommand command = new(query, connection);
        try
        {
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            if (Table?.Rows.Count is not null or > 0)
            {
                Table = new DataTable();
                Table.Reset();
            }
            if (reader.HasRows)
            {
                Table.Load(reader);
                TableFilter();
            }
        }
        catch (Exception e)
        {
            throw e;
        }
        finally
        {
            pleaseWait.Close();
            connection.Close();
            command.Connection.Close();
            SelectedRows.Clear();
        }
    }

    /// <summary>
    /// Filter the DataGrid table in the ReportWindow
    /// </summary>
    private void TableFilter()
    {
        StringBuilder builder = new($"CellType LIKE '%{CellType}%' AND CartNumber LIKE '%{CartNumber}%' ");
        if (!DateFrom.IsNull() && DateTime.TryParse(DateFrom, out DateTime date))
        {
            builder.Append($"AND Date >= '{date}' ");
        }
        if (!DateTo.IsNull() && DateTime.TryParse(DateTo, out date))
        {
            builder.Append($"AND Date <= '{date}' ");
        }
        if (!TrayNumber.IsNull())
        {
            builder.Append($"AND TrayBarcode LIKE '%{TrayNumber}%' ");
        }
        if (!ExecutionTimeFrom.IsNull() && TimeSpan.TryParse(ExecutionTimeFrom, out TimeSpan time))
        {
            builder.Append($"AND TimeStamp >= '{time}' ");
        }
        if (!ExecutionTimeTo.IsNull() && TimeSpan.TryParse(ExecutionTimeTo, out time))
        {
            builder.Append($"AND TimeStamp <= '{time}' ");
        }
        if (Table?.Rows.Count is not null or > 0)
        {
            Table.DefaultView.RowFilter = builder.ToString();
        }
    }

    #endregion
}
