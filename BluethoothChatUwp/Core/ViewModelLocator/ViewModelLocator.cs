using BluethoothChatUwp.Core.ServiceLocator;
using BluethoothChatUwp.ViewModels;
using BluethoothChatUwp.Views;
using SimpleMvvm;

namespace BluethoothChatUwp.Core.ViewModelLocator
{
    internal static class ViewModelLocator
    {
        private static IServiceLocator _serviceLocator;

        public static void Initialize(IServiceLocator serviceLocator)
        {
            Guard.NotNull(serviceLocator, nameof(serviceLocator));

            _serviceLocator = serviceLocator;
        }

        private static IServiceLocator ServiceLocator => _serviceLocator;

        #region ViewModels

        public static object MainPageViewModel => _serviceLocator.Resolve<MainPageViewModel>();
        
        #endregion
    }
}
