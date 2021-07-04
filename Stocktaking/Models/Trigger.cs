using System;
using System.Net;
using System.Text;
using CommonWindows;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows;

namespace Stocktaking.Models
{
    public class Trigger
    {
        private readonly static int sendTimeOut = 5000;
        private readonly static int reciveTimeOut = 5000;
        private readonly static int connectTimeOut = 500;

        public static string PerformTrigger(string address, int port, string request, Action close)
        {
            try
            {
                // Convert from string to IPAddress
                IPAddress iPAddress = IPAddress.Parse(address);

                // Create new IP end point
                IPEndPoint localEndPoint = new(iPAddress, port);

                // Create new socket
                Socket socket = new(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendTimeout = sendTimeOut,
                    ReceiveTimeout = reciveTimeOut
                };

                TryConnection(socket, iPAddress, port);

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
                string countedTray = Trim(Encoding.ASCII.GetString(data));
                                
                // Closing the socket
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

                return countedTray.TrimStart('0');
            }
            catch (ApplicationException e)
            {
                new InfoBox("אישור", e.Message, MessageLevel.Error, FlowDirection.RightToLeft).ShowDialog();
                close();
            }
            catch (Exception e)
            {
                new InfoBox("אישור", e.Message, flow: FlowDirection.LeftToRight).ShowDialog();
            }
            return null;
        }

        /// <summary>
        /// Check if the server is alive
        /// </summary>
        /// <param name="socket">Specified socket to try the connection</param>
        /// <param name="address">The server address to connect</param>
        /// <param name="port">The server port to connect</param>
        private static void TryConnection(Socket socket, IPAddress address, int port)
        {
            IAsyncResult result = socket.BeginConnect(address, port, null, null);
            result.AsyncWaitHandle.WaitOne(connectTimeOut, true);
            if (socket.Connected)
            {
                socket.EndConnect(result);
            }
            else
            {
                socket.Close();
                throw new ApplicationException("לא צלח חיבור למצלמה, התוכנה תיסגר");
            }
        }

        /// <summary>
        /// Trim "junk" characters from the specified string
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Trimed text</returns>
        private static string Trim(string text) => Regex.Replace(text, @"[^\t\r\n -~]", string.Empty);
    }
}
