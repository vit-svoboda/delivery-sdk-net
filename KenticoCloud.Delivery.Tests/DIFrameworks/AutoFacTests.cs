using Autofac;
using Autofac.Extensions.DependencyInjection;
using KenticoCloud.Delivery.Tests.DIFrameworks.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KenticoCloud.Delivery.Tests.DIFrameworks
{
    public class AutoFacTests
    {
        [Fact]
        public void DeliveryClientIsSuccessfullyResolvedFromAutoFacContainer()
        {
            var serviceCollection = DIFrameworksHelper.GetServiceCollection();
            var autoFacContainerBuilder = new ContainerBuilder();

            autoFacContainerBuilder.Populate(serviceCollection);
            var container = autoFacContainerBuilder.Build();
            var provider = new AutofacServiceProvider(container);

            var client = (DeliveryClient)provider.GetService(typeof(IDeliveryClient));

            DIFrameworksHelper.AssertDefaultDeliveryClient(client);
        }


        [Fact]
        public void DeliveryClientIsSuccessfullyResolvedFromCastleWindsorContainer_CustomCodeFirstModelProvider()
        {
            var serviceCollection = DIFrameworksHelper.GetServiceCollection();
            var autoFacContainerBuilder = new ContainerBuilder();
            var modelProvider = new FakeModelProvider();

            serviceCollection.AddSingleton<ICodeFirstModelProvider>(_ => modelProvider);

            autoFacContainerBuilder.Populate(serviceCollection);
            var container = autoFacContainerBuilder.Build();
            var provider = new AutofacServiceProvider(container);

            var client = (DeliveryClient)provider.GetService(typeof(IDeliveryClient));

            DIFrameworksHelper.AssertDeliveryClientWithCustomCodeFirstModelProvider(client, modelProvider);
        }
    }
}
