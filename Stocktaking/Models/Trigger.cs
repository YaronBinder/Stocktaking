﻿using System;
using System.Net;
using System.Text;
using CommonWindows;
using System.Windows;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Stocktaking.Models;

public static class Trigger
{
    #region Private const variables

    // Camera server port
    private const int port = 2120;

    // Time out for the send data from the camera request
    private const int sendTimeOut = 5000;

    // Time out for the recive data from the camera request
    private const int reciveTimeOut = 5000;

    // Time out for the attempt to connect to the camera
    private const int connectTimeOut = 500;

    // Camera server IP address
    private const string address = "192.168.0.1";

    // Trigger word for the camera to perform counting of the batteries
    private const string request = "\x02trigger\x03";

    #endregion

    #region Static methods

    /// <summary>
    /// Execute The TCP Trigger request from the server
    /// </summary>
    /// <returns>The number of batteries in the tray</returns>
    public static string PerformTrigger()
    {
        try
        {
            // Convert from string to IPAddress
            IPAddress iPAddress = IPAddress.Parse(address);

            // Create new IP end point
            IPEndPoint localEndPoint = new(iPAddress, port);

            // Perform connection to the given socket in order to find if there is a connection
            TryConnection(iPAddress, port);

            // Create new socket
            Socket socket = new(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = sendTimeOut,
                ReceiveTimeout = reciveTimeOut
            };

            // Connect to client
            socket.Connect(localEndPoint);

            // Encoding the massage from string to byte array
            byte[] data = Encoding.ASCII.GetBytes(request);

            // Sending the message and getting the size of the array
            int byteSent = socket.Send(data);

            // New byte array to store the message recived from the server
            data = new byte[8];

            // Reciving the message from the client
            do { socket.Receive(data, 0, data.Length, SocketFlags.None); } while (Encoding.ASCII.GetString(data).Contains("OK"));

            // Saving the massage from the client as an integer
            string countedTray = Regex.Replace(Trim(Encoding.ASCII.GetString(data)), @"\D+", string.Empty); ;

            if (countedTray.ToLower().Contains("error") || string.IsNullOrEmpty(countedTray))
            {
                new InfoBox("אישור", "יש להכניס את המצלמה למצב ריצה - אנא פנו למתכנת", flow:FlowDirection.RightToLeft).ShowDialog();
                return "NaN";
            }

            // Closing the socket
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            return countedTray.TrimStart('0');
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
        Socket socket = new(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            SendTimeout = sendTimeOut,
            ReceiveTimeout = reciveTimeOut
        };
        IAsyncResult result = socket.BeginConnect(address, port, null, null);
        result.AsyncWaitHandle.WaitOne(connectTimeOut, true);
        if (socket.Connected)
        {
            socket.EndConnect(result);
            socket.Close();
        }
        else
        {
            socket.Close();
            throw new ApplicationException("החיבור למצלמה לא צלח - התוכנית תיסגר");
        }
    }

    /// <summary>
    /// Trim Unicode characters from the specified string
    /// </summary>
    /// <param name="text"></param>
    /// <returns>Clean and trimed text</returns>
    private static string Trim(string text) => Regex.Replace(text, @"[^\t\r\n -~]", string.Empty);

    #endregion
}
