using FakeItEasy;
using KenticoCloud.Delivery.ResiliencePolicy;
using RichardSzalay.MockHttp;
using Xunit;

namespace KenticoCloud.Delivery.Tests
{
    public class DeliveryClientBuilderTests
    {
        private const string Guid = "e5629811-ddaa-4c2b-80d2-fa91e16bb264";
        private const string PreviewEndpoint = "https://preview-deliver.test.com/";
        
        [Fact]
        public void BuildClWithProjectId_ReturnsDeliveryClientWithProjectIdSet()
        {
            var deliveryClient = (DeliveryClient) DeliveryClientBuilder.WithProjectId(Guid).Build();

            Assert.Equal(Guid, deliveryClient.DeliveryOptions.ProjectId);
        }

        [Fact]
        public void BuildWithDeliveryOptions_ReturnsDeliveryClientWithDeliveryOptions()
        {
            var deliveryClient = (DeliveryClient) DeliveryClientBuilder
                .WithOptions(builder => builder.WithProjectId(Guid).UsePreviewApi("123").WithCustomEndpoint(PreviewEndpoint).Build()).Build();

            Assert.Equal(Guid, deliveryClient.DeliveryOptions.ProjectId);
            Assert.Equal(PreviewEndpoint, deliveryClient.DeliveryOptions.PreviewEndpoint);
        }

        [Fact]
        public void BuildWithOptionalSteps_ReturnsDeliveryClientWithSetInstances()
        {
            var mockCodeFirstModelProvider = A.Fake<ICodeFirstModelProvider>();
            var mockResiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            var mockCodeFirstPropertyMapper = A.Fake<ICodeFirstPropertyMapper>();
            var mockHttp = new MockHttpMessageHandler().ToHttpClient();

            var deliveryClient = (DeliveryClient) DeliveryClientBuilder
                .WithProjectId(Guid)
                .WithHttpClient(mockHttp)
                .WithCodeFirstModelProvider(mockCodeFirstModelProvider)
                .WithCodeFirstPropertyMapper(mockCodeFirstPropertyMapper)
                .WithResiliencePolicyProvider(mockResiliencePolicyProvider)
                .Build();

            Assert.Equal(Guid, deliveryClient.DeliveryOptions.ProjectId);
            Assert.Equal(mockCodeFirstModelProvider, deliveryClient.CodeFirstModelProvider);
            Assert.Equal(mockCodeFirstPropertyMapper, deliveryClient.CodeFirstPropertyMapper);
            Assert.Equal(mockResiliencePolicyProvider, deliveryClient.ResiliencePolicyProvider);
            Assert.Equal(mockHttp, deliveryClient.HttpClient);
        }
    }
}
