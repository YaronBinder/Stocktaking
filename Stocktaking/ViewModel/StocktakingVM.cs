using ClassRunner4_6_1DotNet.common;
using CommonWindows;
using MilBatDBModels.Common;
using Stocktaking.Classes;
using Stocktaking.ViewModel.Base;
using Stocktaking.Views;
using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TB = System.Windows.Controls.TextBlock;
using Trigger = Stocktaking.Models.Trigger;

namespace Stocktaking.ViewModel;
public class StocktakingVM : BaseVM, INotifyPropertyChanged
{
    #region Constructor

    public StocktakingVM(BarcodeManager barcodeManager, Window window, StackPanel historyPanel) : base(window)
    {
        _window = window;
        _barcodeManager = barcodeManager;
        _barcodeManager.BarcodeAction = BarcodeEvent;
        _historyPanel = historyPanel;
    }

    #endregion

    #region Commands

    private ICommand _ReportWindow;
    public ICommand ReportWindow => _ReportWindow ??= new RelayCommand(OpenReportWindow);

    private ICommand _EnterBarcode;
    public ICommand EnterBarcode => _EnterBarcode ??= new RelayCommand(() => BarcodeEvent(Barcode));

    #endregion

    #region Full Properties

    private string _Barcode;
    public string Barcode
    {
        get => _Barcode;
        set
        {
            _Barcode = value;
            OnPropertyChanged(nameof(Barcode));
        }
    }

    // The image of the tray as captured by the `SICK` camera
    private BitmapSource _TrayImage;
    public BitmapSource TrayImage 
    {
        get => _TrayImage;
        set
        {
            _TrayImage = value;
            OnPropertyChanged(nameof(TrayImage));
        }
    }

    #endregion

    #region Variables

    // The cell type
    private string _cellType;

    // The path to the tray image
    private const string _ImagePath = @"C:\Formation\nova\nova_im.png";

    // Main window reference
    private readonly Window _window;

    // Flag to check if the ReportWindow is already opened
    private bool _isReportWindowOpen = false;

    // History stack panel
    private readonly StackPanel _historyPanel;

    // Serial port barcode scanner manager
    private readonly BarcodeManager _barcodeManager;

    #endregion

    #region Properties

    // The number of cells in the tray
    public string CellCount { get; private set; }

    // The number of the cart
    public string CartBarcodeNumber { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Performing communication with the 'SICK' camera and get the number of batteries in the tray
    /// Then save the data to SQL
    /// </summary>
    /// <param name="barcode">The bacode number of the battery tray or cart</param>
    public void BarcodeEvent(string barcode)
    {
        if (barcode?.Length is null or < 1) return;

        TB textBlock = new()
        {
            FontSize = 26F
        };
        TB cuption = new()
        {
            FontSize = 32F,
            FontWeight = FontWeights.Bold,
            TextDecorations = TextDecorations.Underline
        };

        // In case of cart barcode
        if (barcode.Length <= 3 && !ShowMode)
        {
            Barcode = string.Empty;
            _cellType = null;
            while (string.IsNullOrEmpty(_cellType))
            {
                InputWindows batteryTypeInput = new("אנא הכנס/י את דגם התא", "אישור", "יציאה");
                batteryTypeInput.ShowDialog();
                _cellType = batteryTypeInput.ResultValue;
                if (batteryTypeInput.IsUserClosed) return;
            }
            CartBarcodeNumber = barcode;
            cuption.Text = $"עגלה: {barcode}";
            _historyPanel.Children.Add(cuption);
        }
        // In case of tray barcode
        else
        {
            if (CartBarcodeNumber == null && !ShowMode)
            {
                new InfoBox("אישור", "יש להכניס מספר עגלה").ShowDialog();
                return;
            }
            CellCount = Trigger.PerformTrigger();
            textBlock.Text = $"מגש: {barcode}";
            _historyPanel.Children.Add(textBlock);
            if (!ShowMode)
            {
                // Check if the tray from the same day already exist
                if (IsTrayExist(barcode) is StocktakingData stocktakingData)
                {
                    YesNoWindow yesNo = new("המגש כבר קיים במערכת, האם להחליף בינהם?");
                    yesNo.ShowDialog();
                    if (yesNo.ResultYes)
                    {
                        try
                        {
                            UpdateData(stocktakingData);
                            new InfoBox("אישור", "נתוני מגש עודכנו בהצלחה", MessageLevel.OK).ShowDialog();
                        }
                        catch (Exception e)
                        {
                            new InfoBox("אישור", $"עדכון הנתונים כשל\n{e.Message}").ShowDialog();
                        }
                    }
                }
                else
                {
                    while (!InsertStocktaking(barcode))
                    {
                        YesNoWindow yesNoWindow = new("הכנסת הנתונים לשרת לא צלחה, לנסות בשנית?");
                        yesNoWindow.ShowDialog();
                        if (!yesNoWindow.ResultYes) return;
                    }
                }
            }
            GetTrayImage();
        }
    }

    /// <summary>
    /// Get the image as captured by the `SICK` camera
    /// </summary>
    private void GetTrayImage()
    {
        while (Directory.GetFiles(Path.GetDirectoryName(_ImagePath)).Length == 0)
        {
            Thread.Sleep(250);
        }
        Bitmap bitmap = System.Drawing.Image.FromFile(_ImagePath, true) as Bitmap;
        Bitmap image = new(bitmap);
        bitmap.Dispose();
        TrayImage = BitmapToBitmapSource(image);
        File.Delete(_ImagePath);
    }

    /// <summary>
    /// Update the date, time and cell count of the corresponding data
    /// </summary>
    /// <param name="data"><see cref="StocktakingData"/> object with the data from the SQL Server</param>
    /// <returns><see langword="true"/> if the data were updated successfully, otherwise <see langword="false"/></returns>
    private bool UpdateData(StocktakingData data)
    {
        using SqlConnection connection = new(connectionString);
        string query = @"UPDATE [StocktakingData]
                             SET [Date] = @NewDate, [CellCount] = @NewCellCount, [TimeStamp] = @NewTime, [CellType] = @NewCellType
                             WHERE [TrayBarcode] = @TrayBarcode
                             AND [CartNumber] = @CartNumber
                             AND [Date] = @Date";
        try
        {
            connection.Open();
            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@Date", data.date.ToString("d"));
            command.Parameters.AddWithValue("@TrayBarcode", data.tray);
            command.Parameters.AddWithValue("@CartNumber", data.cart);
            command.Parameters.AddWithValue("@NewCellCount", CellCount);
            command.Parameters.AddWithValue("@NewCellType", _cellType);
            command.Parameters.AddWithValue("@NewDate", DateTime.Now.ToString("d"));
            command.Parameters.AddWithValue("@NewTime", DateTime.Now.ToString("T"));
            command.ExecuteNonQuery();
            return true;
        }
        catch (Exception e)
        {
            return false;
            throw e;
        }
        finally
        {
            connection.Close();
        }
    }

    /// <summary>
    /// Check if the current tray already exist
    /// </summary>
    /// <param name="barcode">The specified tray barcode</param>
    /// <returns><see cref="StocktakingData"/> with the data on the tray; Otherwise<see langword="null"/></returns>
    private StocktakingData IsTrayExist(string barcode)
    {
        using SqlConnection connection = new(connectionString);
        string query = @"SELECT TOP 1 * FROM [StocktakingData]
                             WHERE [TrayBarcode] = @TrayBarcode
                             AND [CartNumber] = @CartNumber
                             AND [Date] = @Date";
        try
        {
            connection.Open();
            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@TrayBarcode", barcode);
            command.Parameters.AddWithValue("@CartNumber", CartBarcodeNumber);
            command.Parameters.AddWithValue("@Date", DateTime.Now.ToString("d"));
            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read() && reader.HasRows)
            {
                return new StocktakingData(
                    reader.GetDateTime(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5));
            }
        }
        finally
        {
            connection.Close();
        }
        return null;
    }

    /// <summary>
    /// Insert the given data to the SQL table Stocktaking
    /// </summary>
    /// <param name="barcode">The battrey tray barcode</param>
    /// <param name="cellAmount">The number of cells in the tray</param>
    /// <returns><see langword="true"/> if the insert was successful; Otherwise <see langword="false"/></returns>
    private bool InsertStocktaking(string barcode)
    {
        string query = @"INSERT INTO StocktakingData (Date, CellCount, TimeStamp, TrayBarcode, CartNumber, CellType)
                             VALUES (@Date, @CellCount, @TimeStamp, @TrayBarcode, @CartNumber, @CellType)";
        using SqlConnection connection = new(connectionString);
        connection.Open();
        using SqlTransaction transaction = connection.BeginTransaction();
        using SqlCommand command = new(query, connection, transaction);
        try
        {
            command.Parameters.AddWithValue("@Date", DateTime.Now.ToString("d"));
            command.Parameters.AddWithValue("@CellCount", CellCount);
            command.Parameters.AddWithValue("@TimeStamp", DateTime.Now.ToString("T"));
            command.Parameters.AddWithValue("@TrayBarcode", barcode);
            command.Parameters.AddWithValue("@CartNumber", CartBarcodeNumber);
            command.Parameters.AddWithValue("@CellType", _cellType);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (Exception e)
        {
            transaction.Rollback();
            return false;
        }
        finally
        {
            connection.Close();
            command.Connection.Close();
            transaction.Connection?.Close();
        }
        return true;
    }

    /// <summary>
    /// Open new <see cref="ReportWindow"/>
    /// </summary>
    private void OpenReportWindow()
    {
        if (_isReportWindowOpen) return;
        _isReportWindowOpen = true;
        _window.WindowState = WindowState.Minimized;
        Thread reportWindowThread = new(new ThreadStart(new Action(() =>
        {
            ReportWindow reportWindow = new();
            reportWindow.Closed += delegate
            {
                _isReportWindowOpen = false;
                Application.Current.Dispatcher?.Invoke(
                    new Action(() =>
                    {
                        _window.WindowState = WindowState.Normal;
                    }));
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
            };
            reportWindow.ShowDialog();
            Dispatcher.Run();
        }))){ IsBackground = true };
        reportWindowThread.SetApartmentState(ApartmentState.STA);
        reportWindowThread.Start();
    }

    /// <summary>
    /// Convert <see cref="Bitmap"/> image to <see cref="BitmapSource"/>
    /// </summary>
    /// <param name="source">The <see cref="Bitmap"/> image to convert</param>
    /// <returns>The converted <see cref="Bitmap"/> image as <see cref="BitmapSource"/></returns>
    public static BitmapSource BitmapToBitmapSource(Bitmap source)
        => System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
           source.GetHbitmap(),
           IntPtr.Zero,
           Int32Rect.Empty,
           BitmapSizeOptions.FromEmptyOptions());
    #endregion
}