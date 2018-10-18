using KenticoCloud.Delivery.CodeFirst;
using KenticoCloud.Delivery.ContentLinks;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace KenticoCloud.Delivery.Tests.DIFrameworks.Helpers
{
    internal static class DIFrameworksHelper
    {
        public static string ProjectId { get; } = "1324";

        public static IServiceCollection GetServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDeliveryClient(new DeliveryOptions() { ProjectId = ProjectId });

            return serviceCollection;
        }

        public static void AssertDefaultDeliveryClient(DeliveryClient client)
        {
            AssertDeliveryClientDependencies(client);
            Assert.NotNull(client.CodeFirstModelProvider);
            Assert.Equal(ProjectId, client.DeliveryOptions.ProjectId);

            Assert.IsType<DefaultTypeProvider>(client.CodeFirstTypeProvider);
            Assert.IsType<DefaultContentLinkUrlResolver>(client.ContentLinkUrlResolver);
        }

        public static void AssertDeliveryClientWithCustomCodeFirstModelProvider(DeliveryClient client, ICodeFirstModelProvider modelProvider)
        {
            AssertDeliveryClientDependencies(client);
            Assert.Equal(modelProvider, client.CodeFirstModelProvider);
        }

        private static void AssertDeliveryClientDependencies(DeliveryClient client)
        {
            Assert.NotNull(client);
            Assert.NotNull(client.CodeFirstPropertyMapper);
            Assert.NotNull(client.CodeFirstTypeProvider);
            Assert.NotNull(client.ContentLinkUrlResolver);
            Assert.NotNull(client.DeliveryOptions);
        }
    }
}   
