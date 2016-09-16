using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BluethoothChatUwp.Models
{
    public interface IRfcommChatServer
        : INotifyPropertyChanged
    {
        string UserName { get; }
        string CompanyName { get; }

        int Summ(int a, int b);

        void Initialize();

        void SendMessage(string strMessage);

        void Disconnect();
    }
}
