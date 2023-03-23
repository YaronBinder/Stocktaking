using System;
using System.Net;
using System.Text;
using CommonWindows;
using System.Windows;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Stocktaking.Classes;

namespace Stocktaking.Models;

public static class Trigger
{
    #region Private const variables

    /// <summary>
    /// Camera server port
    /// </summary>
    private const int PORT = 2120;

    /// <summary>
    /// Not a number
    /// </summary>
    private const string NaN = "NaN";

    /// <summary>
    /// Time out for the send data from the camera request
    /// </summary>
    private const int SEND_TIME_OUT = 5000;

    /// <summary>
    /// Time out for the recive data from the camera request
    /// </summary>
    private const int RECIVE_TIME_OUT = 5000;

    /// <summary>
    /// Time out for the attempt to connect to the camera
    /// </summary>
    private const int CONNECTION_TIME_OUT = 500;

    /// <summary>
    /// Camera server IP address
    /// </summary>
    private const string ADDRESS = "192.168.0.1";

    /// <summary>
    /// Trigger word for the camera to perform counting of the batteries
    /// </summary>
    private const string TRIGGER = "\x02trigger\x03";

    #endregion

    #region Static methods

    /// <summary>
    /// Execute The TCP Trigger request from the server
    /// </summary>
    /// <returns>The number of batteries in the tray</returns>
    public static TrayInfo PerformTrigger()
    {
        try
        {
            // Convert from string to IPAddress
            IPAddress iPAddress = IPAddress.Parse(ADDRESS);

            // Create new IP end point
            IPEndPoint localEndPoint = new(iPAddress, PORT);

            // Perform connection to the given socket in order to find if there is a connection
            TryConnection(iPAddress, PORT);

            // Create new socket
            using Socket socket = new(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = SEND_TIME_OUT,
                ReceiveTimeout = RECIVE_TIME_OUT
            };

            // Connect to client
            socket.Connect(localEndPoint);

            // Encoding the massage from string to byte array
            byte[] data = Encoding.ASCII.GetBytes(TRIGGER);

            // Sending the message and getting the size of the array
            int byteSent = socket.Send(data);

            // New byte array to store the message recived from the server
            data = new byte[10];

            // Reciving the message from the client
            do
            {
                socket.Receive(data, 0, data.Length, SocketFlags.None);
            } while (Encoding.ASCII.GetString(data).Contains("OK"));

            // Saving the massage from the client as tray battery count
            // And bool for knowing if ther is a tray
            string[] received = Trim(Encoding.ASCII.GetString(data)).Split('_');

            // The amount of batteries in the tray
            string countedTray = TrimNonNumber(received[0]);

            // True if no tray is placed in, false otherwize
            bool IsNoTray = bool.Parse(received[1]);

            // In case The camera isn't set to 'Run' in the configurator at 192.168.0.1
            if (countedTray.ToLower().Contains("error") || string.IsNullOrEmpty(countedTray))
            {
                new InfoBox("אישור", "יש להכניס את המצלמה למצב ריצה - אנא פנו לטכנאי", flow: FlowDirection.RightToLeft).ShowDialog();
                countedTray = NaN;
            }

            // In case theere is no tray placed in
            if (IsNoTray)
            {
                new InfoBox("אישור", "אנא הכנס מגש").ShowDialog();
                countedTray = NaN;
            }

            // Closing the socket
            socket.Shutdown(SocketShutdown.Both);
            return new(countedTray.TrimStart('0'), IsNoTray);
        }
        catch (ApplicationException e)
        {
            new InfoBox("אישור", e.Message, MessageLevel.Error, FlowDirection.RightToLeft).ShowDialog();
            Environment.Exit(1);
        }
        catch (Exception e)
        {
            new InfoBox("אישור", e.Message, flow: FlowDirection.LeftToRight).ShowDialog();
            Environment.Exit(1);
        }
        return null;
    }

    /// <summary>
    /// Check if the server is alive
    /// </summary>
    /// <param name="address">The server address to connect</param>
    /// <param name="port">The server port to connect</param>
    private static void TryConnection(IPAddress address, int port)
    {
        // Create new socket
        using Socket socket = new(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            SendTimeout = SEND_TIME_OUT,
            ReceiveTimeout = RECIVE_TIME_OUT
        };
        IAsyncResult result = socket.BeginConnect(address, port, null, null);
        result.AsyncWaitHandle.WaitOne(CONNECTION_TIME_OUT, true);
        if (socket.Connected)
        {
            socket.EndConnect(result);
        }
        else
        {
            throw new ApplicationException("החיבור למצלמה לא צלח - התוכנית תיסגר");
        }
    }

    /// <summary>
    /// Trim Unicode characters from the specified string
    /// </summary>
    /// <param name="text">The text to trim</param>
    /// <returns>Clean and trimed text</returns>
    private static string Trim(string text) => Regex.Replace(text, @"[^\t\r\n -~]", string.Empty);

    /// <summary>
    /// Trim non number characters from the specified string
    /// </summary>
    /// <param name="text">The text to trim</param>
    /// <returns>Clean and trimed text</returns>
    private static string TrimNonNumber(string text) => Regex.Replace(text, @"\D+", string.Empty);

    #endregion
}