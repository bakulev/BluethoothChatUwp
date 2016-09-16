using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BluethoothChatUwp.Models
{
    public interface IRfcommChatClient
        : INotifyPropertyChanged
    {
        void Run();

        void SelectDevice(int iSelected);

        void SendMessage(string strMessage);

        void Disconnect(string disconnectReason);
    }
}
