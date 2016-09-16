using System;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using BluethoothChatUwp.Core;
using SimpleMvvm;
using SimpleMvvm.Annotations;

namespace BluethoothChatUwp.Models
{
    internal class RfcommChatServer
        : ObservableObject, IRfcommChatServer
    {
        /// <summary>
        /// Class containing Attributes and UUIDs that will populate the SDP record.
        /// </summary>
        private class Constants
        {
            // The Chat Server's custom service Uuid: 34B1CF4D-1069-4AD6-89B6-E161D79BE4D8
            public static readonly Guid RfcommChatServiceUuid = Guid.Parse("34B1CF4D-1069-4AD6-89B6-E161D79BE4D8");

            // The Id of the Service Name SDP attribute
            public const UInt16 SdpServiceNameAttributeId = 0x100;

            // The SDP Type of the Service Name SDP attribute.
            // The first byte in the SDP Attribute encodes the SDP Attribute Type as follows :
            //    -  the Attribute Type size in the least significant 3 bits,
            //    -  the SDP Attribute Type value in the most significant 5 bits.
            public const byte SdpServiceNameAttributeType = (4 << 3) | 5;

            // The value of the Service Name SDP attribute
            public const string SdpServiceName = "Bluetooth Rfcomm Chat Service";
        }

        private StreamSocket socket;
        private DataWriter writer;
        private RfcommServiceProvider rfcommProvider;
        private StreamSocketListener socketListener;

        /*public Model(IState state, IUIService uiService)
        {
            Guard.NotNull(state, nameof(state));

            _state = state;
            _spectrometer = new Spectrometer(uiService);
        }*/

        #region Properties

        #region UserName

        private string _userName;

        public string UserName
        {
            get { return _userName; }
            private set { SetValue(ref _userName, value); }
        }

        #endregion

        #region CompanyName

        private string _companyName;
        public string CompanyName
        {
            get { return _companyName; }
            private set { SetValue(ref _companyName, value); }
        }

        #endregion

        #endregion

        public void SetRegistrationData(string userName, string companyName)
        {
            UserName = userName;
            CompanyName = companyName;
        }

        public int Summ(int a, int b)
        {
            return a + b;
        }

        /// <summary>
        /// Initializes the server using RfcommServiceProvider to advertise the Chat Service UUID and start listening
        /// for incoming connections.
        /// </summary>
        public async void Initialize()
        {
            rfcommProvider = await RfcommServiceProvider.CreateAsync(
                RfcommServiceId.FromUuid(Constants.RfcommChatServiceUuid));

            // Create a listener for this service and start listening
            socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += OnConnectionReceived;

            await socketListener.BindServiceNameAsync(rfcommProvider.ServiceId.AsString(),
                SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

            // Set the SDP attributes and start Bluetooth advertising
            InitializeServiceSdpAttributes(rfcommProvider);
            rfcommProvider.StartAdvertising(socketListener);
        }

        /// <summary>
        /// Creates the SDP record that will be revealed to the Client device when pairing occurs.  
        /// </summary>
        /// <param name="rfcommProvider">The RfcommServiceProvider that is being used to initialize the server</param>
        private void InitializeServiceSdpAttributes(RfcommServiceProvider rfcommProvider)
        {
            var sdpWriter = new DataWriter();

            // Write the Service Name Attribute.
            sdpWriter.WriteByte(Constants.SdpServiceNameAttributeType);

            // The length of the UTF-8 encoded Service Name SDP Attribute.
            sdpWriter.WriteByte((byte)Constants.SdpServiceName.Length);

            // The UTF-8 encoded Service Name value.
            sdpWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            sdpWriter.WriteString(Constants.SdpServiceName);

            // Set the SDP Attribute on the RFCOMM Service Provider.
            rfcommProvider.SdpRawAttributes.Add(Constants.SdpServiceNameAttributeId, sdpWriter.DetachBuffer());
        }

        public async void SendMessage(string strMessage)
        {
            // There's no need to send a zero length message
            if (strMessage.Length != 0)
            {
                // Make sure that the connection is still up and there is a message to send
                if (socket != null)
                {
                    writer.WriteUInt32((uint)strMessage.Length);
                    writer.WriteString(strMessage);
                    
                    await writer.StoreAsync();

                }
                else
                {
                    
                }
            }
        }

        public async void Disconnect()
        {
            if (rfcommProvider != null)
            {
                rfcommProvider.StopAdvertising();
                rfcommProvider = null;
            }

            if (socketListener != null)
            {
                socketListener.Dispose();
                socketListener = null;
            }

            if (writer != null)
            {
                writer.DetachStream();
                writer = null;
            }

            if (socket != null)
            {
                socket.Dispose();
                socket = null;
            }
        }

        /// <summary>
        /// Invoked when the socket listener accepts an incoming Bluetooth connection.
        /// </summary>
        /// <param name="sender">The socket listener that accepted the connection.</param>
        /// <param name="args">The connection accept parameters, which contain the connected socket.</param>
        private async void OnConnectionReceived(
            StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            // Don't need the listener anymore
            socketListener.Dispose();
            socketListener = null;

            socket = args.Socket;

            writer = new DataWriter(socket.OutputStream);

            var reader = new DataReader(socket.InputStream);
            bool remoteDisconnection = false;

            // Infinite read buffer loop
            while (true)
            {
                try
                {
                    // Based on the protocol we've defined, the first uint is the size of the message
                    uint readLength = await reader.LoadAsync(sizeof(uint));

                    // Check if the size of the data is expected (otherwise the remote has already terminated the connection)
                    if (readLength < sizeof(uint))
                    {
                        remoteDisconnection = true;
                        break;
                    }
                    uint currentLength = reader.ReadUInt32();
                    //if (currentLength > 16) currentLength = 16;

                    // Load the rest of the message since you already know the length of the data expected.  
                    readLength = await reader.LoadAsync(currentLength);

                    // Check if the size of the data is expected (otherwise the remote has already terminated the connection)
                    if (readLength < currentLength)
                    {
                        remoteDisconnection = true;
                        break;
                    }
                    string message = reader.ReadString(currentLength);

                }
                catch (Exception ex)
                {
                    switch ((uint)ex.HResult)
                    {
                        case (0x800703E3):

                            break;
                        default:
                            throw;
                    }
                    break;

                }
            }

            reader.DetachStream();
            if (remoteDisconnection)
            {
                Disconnect();

            }
        }
    }
}
