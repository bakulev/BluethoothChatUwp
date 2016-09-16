using System.Diagnostics;

namespace BluethoothChatUwp.Core.ServiceLocator
{
    public interface IServiceLocator
    {
        [DebuggerStepThrough]
        T Resolve<T>();
    }
}
