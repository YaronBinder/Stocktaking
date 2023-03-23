using ActiveDirectoryLogin.View;
using ActiveDirectoryLogin.ViewModels;
using ClassRunner;
using CommonWindows;
using DevTools;
using Microsoft.Office.Interop.Excel;
using MilBatDBModels.Common;
using Stocktaking.Models;
using Stocktaking.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DataTable = System.Data.DataTable;
using Excel = Microsoft.Office.Interop.Excel;
using Window = System.Windows.Window;

namespace Stocktaking.ViewModel;

public class ReportWindowVM : BaseVM, INotifyPropertyChanged
{
    #region Constructor

    public ReportWindowVM(Window window, DataGrid dataGridTable) : base(window)
    {
        new PleaseWait("טוען נתונים", actions: TableFiller).ShowDialog();
        _dataGrid = dataGridTable;
    }

    #endregion

    #region Properties

    public bool IsNotInProcess { get; set; } = true;
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
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
        }
    }

    #endregion

    #region Commands

    private RelayCommand _ExcelReport;
    public RelayCommand ExcelReport => _ExcelReport ??= new RelayCommand(CreateExcelReport);

    private RelayCommand _RefreshTable;
    public RelayCommand RefreshTable => _RefreshTable ??= new RelayCommand(TableFiller);

    private RelayCommand _DeleteRow;
    public RelayCommand DeleteRow => _DeleteRow ??= new RelayCommand(DeleteRowAction);

    #endregion

    #region Methods

    private void DeleteRowAction()
    {
        if (SelectedRows.Count == 0)
        {
            new InfoBox("אישור", "יש לבחור רשומה/ות למחיקה", MessageLevel.OK).ShowDialog();
            return;
        }
        string verificationMessage = SelectedRows.Count > 1 ? $"האם למחוק {SelectedRows.Count} רשומות אלו?" : "האם למחוק רשומה זו?";
        YesNoWindow Verification = new(verificationMessage);
        Verification.ShowDialog();
        if (!Verification.ResultYes)
        {
            return;
        }
        Login login = new(AppName);
        login.ShowDialog();
        ADLoginVM loginVM = (ADLoginVM)login.DataContext;
        if (loginVM.IsSuccessfulLogin)
        {
            DeleteData();
        }
    }

    private void DeleteData()
    {
        int effectedRows = default;
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
        TableFiller();
        new InfoBox("אישור", $"{effectedRows} שדות נמחקו בהצלחה", MessageLevel.OK).ShowDialog();
    }

    /// <summary>
    /// Create excel sheet report using the selected rows or the entire <see cref="Table"/>
    /// </summary>
    private void CreateExcelReport()
    {
        IsNotInProcess = false;
        if (Table.Rows.Count == 0)
        {
            new InfoBox("אישור", "אין נתונים בטבלה").ShowDialog();
            return;
        }

        if (SelectedRows.Count <= 1)
        {
            YesNoWindow yesNoWindow = new("האם ברצונך להוציא דו\"ח של כל הטבלה?");
            yesNoWindow.ShowDialog();
            if (!yesNoWindow.ResultYes)
            {
                return;
            }
        }
        new PleaseWait("מפיק דוח - אנא המתן", actions: CreateExcel).ShowDialog();
    }

    private void CreateExcel()
    {
        Range range;
        _Workbook workbook;
        _Worksheet worksheet;
        Excel.Application excelApp;
        Thread createReport = new(() =>
        {
            try
            {
                excelApp = new()
                {
                    Visible = false,
                    UserControl = false
                };

                if (excelApp == null)
                {
                    new InfoBox("אישור", "יש להתקין EXCEL במשב זה", MessageLevel.Error).ShowDialog();
                    return;
                }

                // Stacktaking table columns name
                string[] columns = new string[] { "CellType", "CartNumber", "TrayBarcode", "CellCount", "Date", "TimeStamp" };

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
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
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

                worksheet.Rows[3].Select();
                worksheet.Application.ActiveWindow.FreezePanes = true;

                // Style for the columns
                range = worksheet.get_Range("A2", "F2");
                range.Font.Size = 22;
                range.EntireColumn.AutoFit();
                range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
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
                    string color = i % 2 == 0 ? "#50CFAF" : "#50AFCF";
                    // Style
                    range = worksheet.get_Range($"A{cellIndex}", $"F{cellIndex}");
                    range.Font.Size = 16;
                    range.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    range.HorizontalAlignment = Excel.XlVAlign.xlVAlignCenter;
                    range.Interior.Color = ColorTranslator.FromHtml(color);

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
                Logger.WriteLog(e, AppName);
            }
            finally
            {
                IsNotInProcess = true;
            }
            SelectedRows.Clear();
        });
        createReport.SetApartmentState(ApartmentState.STA);
        createReport.Start();
        createReport.Join();
    }

    /// <summary>
    /// Get the whole Stocktaking table 
    /// </summary>
    /// <returns>The whole Stocktaking table</returns>
    private void TableFiller()
    {
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
            if (reader.Read() && reader.HasRows)
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
