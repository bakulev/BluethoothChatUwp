using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using BluethoothChatUwp.Core;
using BluethoothChatUwp.Models;
using SimpleMvvm;
using SimpleMvvm.Commands;
using SimpleMvvm.Common;

namespace BluethoothChatUwp.ViewModels
{
    internal class MainPageViewModel
        : ViewModel
    {
        private readonly IRfcommChatClient _model;

        public MainPageViewModel(IRfcommChatClient model)
		{
            Guard.NotNull(model, nameof(model));

            _model = model;
        }

        #region Values

        private string _message = "";
        private string _chatlog = "";
 
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (value != _message)
                {
                    _message = value;
                    OnPropertyChanged("Message");
                }
            }
        }

        public string ChatLog
        {
            get
            {
                return _chatlog;
            }
            set
            {
                if (value != _chatlog)
                {
                    _chatlog = value;
                    OnPropertyChanged("ChatLog");
                }
            }
        }

        #endregion

        #region Commands

        #region RunCommand

        private AsyncCommand _runCommand;

        public AsyncCommand RunCommand => GetCommand(ref _runCommand, DoRun);

        private async Task DoRun()
        {
            _model.Run();
            return;
        }

        #endregion

        #region DisconnectCommand

        private AsyncCommand _disconnectCommand;

        public AsyncCommand DisconnectCommand => GetCommand(ref _disconnectCommand, DoDisconnect);

        private async Task DoDisconnect()
        {
            _model.Disconnect("Button pressed");
            return;
        }

        #endregion

        #region SendCommand

        private AsyncCommand _sendCommand;

        public AsyncCommand SendCommand => GetCommand(ref _sendCommand, DoSend);

        private async Task DoSend()
        {
            _model.SendMessage(_message);
            return;
        }

        #endregion

        #endregion
    }
}
