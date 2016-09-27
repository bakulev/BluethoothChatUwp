using System;
using System.Collections.Generic;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using BluethoothChatUwp.Core;
using SimpleMvvm;
using SimpleMvvm.Annotations;

namespace BluethoothChatUwp.Models
{
    internal class RfcommChatClient
        : ObservableObject, IRfcommChatClient
    {
        /*public Model(IState state, IUIService uiService)
        {
            Guard.NotNull(state, nameof(state));

            _state = state;
            _spectrometer = new Spectrometer(uiService);
        }*/

        private StreamSocket chatSocket = null;
        private DataWriter chatWriter = null;
        private RfcommDeviceService chatService = null;
        private DeviceInformationCollection chatServiceDeviceCollection = null;

        /// <summary>
        /// Class containing Attributes and UUIDs that will populate the SDP record.
        /// </summary>
        class Constants
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

        /// <summary>
        /// When the user presses the run button, check to see if any of the currently paired devices support the Rfcomm chat service and display them in a list.  
        /// Note that in this case, the other device must be running the Rfcomm Chat Server before being paired.  
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the event.</param>
        public async void Run()
        {
             // Find all paired instances of the Rfcomm chat service and display them in a list
            RfcommServiceId serid = RfcommServiceId.FromUuid(Constants.RfcommChatServiceUuid);
            string stsel = RfcommDeviceService.GetDeviceSelector(serid);
            string stsel1 = "System.Devices.InterfaceClassGuid:=\"{B142FC3E-FA4E-460B-8ABC-072B628B3C70}\" AND System.DeviceInterface.Bluetooth.ServiceGuid:=\"{34B1CF4D-1069-4AD6-89B6-E161D79BE4D8}\"";
            string stsel2 = "System.Devices.InterfaceClassGuid:=\"{B142FC3E-FA4E-460B-8ABC-072B628B3C70}\"";
            string stsel3 = "";
            chatServiceDeviceCollection = await DeviceInformation.FindAllAsync(stsel);

            int i = 0;
            if (chatServiceDeviceCollection.Count > 0)
            {
                //DeviceList.Items.Clear();
                foreach (var chatServiceDevice in chatServiceDeviceCollection)
                {
                    //if (chatServiceDevice.Name == "BAKULEV-X240")
                    {
                        //DeviceList.Items.Add(chatServiceDevice.Name + " " + chatServiceDevice.Id);
                        i++;
                    }
                }
                //DeviceList.Visibility = Windows.UI.Xaml.Visibility.Visible;
                SelectDevice(0); //bakulev
            }
            else
            {
                //rootPage.NotifyUser(
                //    "No chat services were found. Please pair with a device that is advertising the chat service.",
                //    NotifyType.ErrorMessage);
            }

        }

        /// <summary>
        /// Invoked once the user has selected the device to connect to.  
        /// Once the user has selected the device, 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void SelectDevice(int iSelected)
        {
            var chatServiceDevice = chatServiceDeviceCollection[iSelected];
            chatService = await RfcommDeviceService.FromIdAsync(chatServiceDevice.Id);

            if (chatService == null)
            {
                //rootPage.NotifyUser(
                //    "Access to the device is denied because the application was not granted access",
                //    NotifyType.StatusMessage);
                return;
            }

            // Do various checks of the SDP record to make sure you are talking to a device that actually supports the Bluetooth Rfcomm Chat Service 
            var attributes = await chatService.GetSdpRawAttributesAsync();
            if (!attributes.ContainsKey(Constants.SdpServiceNameAttributeId))
            {
                //rootPage.NotifyUser(
                //    "The Chat service is not advertising the Service Name attribute (attribute id=0x100). " +
                //    "Please verify that you are running the BluetoothRfcommChat server.",
                //    NotifyType.ErrorMessage);
                return;
            }

            var attributeReader = DataReader.FromBuffer(attributes[Constants.SdpServiceNameAttributeId]);
            var attributeType = attributeReader.ReadByte();
            if (attributeType != Constants.SdpServiceNameAttributeType)
            {
                //rootPage.NotifyUser(
                //    "The Chat service is using an unexpected format for the Service Name attribute. " +
                //    "Please verify that you are running the BluetoothRfcommChat server.",
                //    NotifyType.ErrorMessage);
                return;
            }

            var serviceNameLength = attributeReader.ReadByte();

            // The Service Name attribute requires UTF-8 encoding.
            attributeReader.UnicodeEncoding = UnicodeEncoding.Utf8;
            //ServiceName.Text = "Service Name: \"" + attributeReader.ReadString(serviceNameLength) + "\"";

            lock (this)
            {
                chatSocket = new StreamSocket();
            }
            try
            {
                await chatSocket.ConnectAsync(chatService.ConnectionHostName, chatService.ConnectionServiceName);

                chatWriter = new DataWriter(chatSocket.OutputStream);
                //ChatBox.Visibility = Windows.UI.Xaml.Visibility.Visible;

                DataReader chatReader = new DataReader(chatSocket.InputStream);
                ReceiveStringLoop(chatReader);
            }
            catch (Exception ex)
            {
                switch ((uint)ex.HResult)
                {
                    case (0x80070490): // ERROR_ELEMENT_NOT_FOUND
                        //rootPage.NotifyUser("Please verify that you are running the BluetoothRfcommChat server.", NotifyType.ErrorMessage);
                        //RunButton.IsEnabled = true;
                        break;
                    default:
                        throw;
                }
            }
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        /// <summary>
        /// Takes the contents of the MessageTextBox and writes it to the outgoing chatWriter
        /// </summary>
        public async void SendMessage(string strMessage)
        {
            try
            {
                if (strMessage.Length != 0 && chatWriter != null)
                {
                    byte[] byteStr = GetBytes(strMessage);
                    int strLen = byteStr.Length;
                    // Write string lentgth first.
                    chatWriter.WriteBytes(BitConverter.GetBytes(strLen));
                    // Write base64 encoded string
                    chatWriter.WriteBytes(byteStr);

                    //ConversationList.Items.Add("Sent: " + strMessage);
                    strMessage = "";
                    await chatWriter.StoreAsync();

                }
            }
            catch (Exception ex)
            {
                // TODO: Catch disconnect -  HResult = 0x80072745 - catch this (remote device disconnect) ex = {"An established connection was aborted by the software in your host machine. (Exception from HRESULT: 0x80072745)"}
                //rootPage.NotifyUser("Error: " + ex.HResult.ToString() + " - " + ex.Message,
                //    NotifyType.StatusMessage);
            }
        }

        private async void ReceiveStringLoop(DataReader chatReader)
        {
            try
            {
                uint size = await chatReader.LoadAsync(sizeof(uint));
                if (size < sizeof(uint))
                {
                    Disconnect("Remote device terminated connection");
                    return;
                }

                uint stringLength = chatReader.ReadUInt32();
                uint actualStringLength = await chatReader.LoadAsync(stringLength);
                if (actualStringLength != stringLength)
                {
                    // The underlying socket was closed before we were able to read the whole data
                    return;
                }

                //ConversationList.Items.Add("Received: " + chatReader.ReadString(stringLength));

                ReceiveStringLoop(chatReader);
            }
            catch (Exception ex)
            {
                lock (this)
                {
                    if (chatSocket == null)
                    {
                        // Do not print anything here -  the user closed the socket.
                        // HResult = 0x80072745 - catch this (remote device disconnect) ex = {"An established connection was aborted by the software in your host machine. (Exception from HRESULT: 0x80072745)"}
                    }
                    else
                    {
                        Disconnect("Read stream failed with error: " + ex.Message);
                    }
                }
            }
        }

         /// <summary>
        /// Cleans up the socket and DataWriter and reset the UI
        /// </summary>
        /// <param name="disconnectReason"></param>
        public void Disconnect(string disconnectReason)
        {
            if (chatWriter != null)
            {
                chatWriter.DetachStream();
                chatWriter = null;
            }


            if (chatService != null)
            {
                chatService.Dispose();
                chatService = null;
            }
            lock (this)
            {
                if (chatSocket != null)
                {
                    chatSocket.Dispose();
                    chatSocket = null;
                }
            }

        }
    }
}
