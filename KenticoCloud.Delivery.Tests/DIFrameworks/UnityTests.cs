using KenticoCloud.Delivery.Tests.DIFrameworks.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Unity;
using Unity.Microsoft.DependencyInjection;
using Xunit;

namespace KenticoCloud.Delivery.Tests.DIFrameworks
{
    public class UnityTests
    {
        [Fact]
        public void DeliveryClientIsSuccessfullyResolvedFromUnityContainer()
        {
            var container = new UnityContainer();
            var serviceCollection = DIFrameworksHelper.GetServiceCollection();
            var provider = serviceCollection.BuildServiceProvider(container);

            var client = (DeliveryClient)provider.GetService<IDeliveryClient>();

            DIFrameworksHelper.AssertDefaultDeliveryClient(client);
        }

        [Fact]
        public void DeliveryClientIsSuccessfullyResolvedFromUnityContainer_CustomCodeFirstModelProvider()
        {
            var container = new UnityContainer();
            var modelProvider = new FakeModelProvider();
            var serviceCollection = DIFrameworksHelper.GetServiceCollection();

            serviceCollection.AddSingleton<ICodeFirstModelProvider>(_ => modelProvider);

            var provider = serviceCollection.BuildServiceProvider(container);

            var client = (DeliveryClient)provider.GetService<IDeliveryClient>();

            DIFrameworksHelper.AssertDeliveryClientWithCustomCodeFirstModelProvider(client, modelProvider);
        }
    }
}
