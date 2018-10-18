using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using KenticoCloud.Delivery.Tests.DIFrameworks.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KenticoCloud.Delivery.Tests.DIFrameworks
{
    public class CastleWindsorTests
    {
        [Fact]
        public void DeliveryClientIsSuccessfullyResolvedFromCastleWindsorContainer()
        {
            var serviceCollection = DIFrameworksHelper.GetServiceCollection();
            var castleContainer = new WindsorContainer();
            var provider = WindsorRegistrationHelper.CreateServiceProvider(castleContainer, serviceCollection);

            var client = (DeliveryClient)provider.GetService<IDeliveryClient>();

            DIFrameworksHelper.AssertDefaultDeliveryClient(client);
        }

        [Fact]
        public void DeliveryClientIsSuccessfullyResolvedFromCastleWindsorContainer_CustomCodeFirstModelProvider()
        {
            var castleContainer = new WindsorContainer();
            var modelProvider = new FakeModelProvider();
            var serviceCollection = DIFrameworksHelper.GetServiceCollection();

            serviceCollection.AddSingleton<ICodeFirstModelProvider>(_ => modelProvider);

            var provider = WindsorRegistrationHelper.CreateServiceProvider(castleContainer, serviceCollection);

            var client = (DeliveryClient)provider.GetService<IDeliveryClient>();

            DIFrameworksHelper.AssertDeliveryClientWithCustomCodeFirstModelProvider(client, modelProvider);
        }
    }
}
