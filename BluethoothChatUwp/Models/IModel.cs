using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BluethoothChatUwp.Models
{
    public interface IModel
        : INotifyPropertyChanged
    {
        string UserName { get; }
        string CompanyName { get; }

        int Summ(int a, int b);
    }
}
