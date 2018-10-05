using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using KenticoCloud.Delivery.InlineContentItems;
using KenticoCloud.Delivery.ResiliencePolicy;
using Microsoft.Extensions.Options;
using Xunit;

namespace KenticoCloud.Delivery.Tests.Extensions
{
    public class ServiceCollectionsExtensionsTests
    {
        private const string Guid = "d79786fb-042c-47ec-8e5c-beaf93e38b84";
        private readonly List<Type> _expectedImplementationTypes = new List<Type>
        {
            typeof(IOptions<DeliveryOptions>),
            typeof(IContentLinkUrlResolver),
            typeof(ICodeFirstTypeProvider),
            typeof(HttpClient),
            typeof(IInlineContentItemsResolver<object>),
            typeof(IInlineContentItemsResolver<UnretrievedContentItem>),
            typeof(IInlineContentItemsProcessor),
            typeof(ICodeFirstModelProvider),
            typeof(ICodeFirstPropertyMapper),
            typeof(IResiliencePolicyProvider),
            typeof(IDeliveryClient)
        };

        [Fact]
        public void AddDeliveryClient()
        {
            var fakeServiceCollection = new FakeServiceCollection();

            ServiceCollectionExtensions.AddDeliveryClient(fakeServiceCollection, new DeliveryOptions{ProjectId = Guid});

            Assert.Empty(_expectedImplementationTypes.Except(fakeServiceCollection.Dependencies.Keys));
            Assert.Empty(fakeServiceCollection.Dependencies.Keys.Except(_expectedImplementationTypes));
        }
    }
}
