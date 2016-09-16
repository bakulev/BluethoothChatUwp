using Autofac;
using Autofac.Builder;
using BluethoothChatUwp.Models;
using BluethoothChatUwp.ViewModels;
using BluethoothChatUwp.Views;
using SimpleMvvm;

namespace BluethoothChatUwp.Core.ServiceLocator
{
    internal class ServiceLocator
        : IServiceLocator
    {
        private readonly IContainer _container;

        public ServiceLocator()
        {
            var builder = new ContainerBuilder();

            RegisterServices(builder);
            RegisterViewModels(builder);

            _container = builder.Build(ContainerBuildOptions.ExcludeDefaultModules);
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        private void RegisterServices(ContainerBuilder builder)
        {
            RegisterSingleton<IMessageHub, MessageHub>(builder);
            RegisterSingleton<IRfcommChatClient, Models.RfcommChatClient>(builder);
        }

        private static void RegisterViewModels(ContainerBuilder builder)
        {
            RegisterViewModel<MainPageViewModel>(builder);
        }

        private static void RegisterSingleton<TService, TImpl>(ContainerBuilder builder)
            where TImpl : TService
        {
            Guard.NotNull(builder, nameof(builder));

            builder
                .RegisterType<TImpl>()
                .As<TService>()
                .SingleInstance();
        }

        private static void RegisterViewModel<TViewModel>(ContainerBuilder builder)
        //    where TViewModel : IrxViewModel
        {
            Guard.NotNull(builder, nameof(builder));

            builder
                .RegisterType<TViewModel>()
                .As<TViewModel>()
                .InstancePerDependency()
                .ExternallyOwned();
        }
    }
}
