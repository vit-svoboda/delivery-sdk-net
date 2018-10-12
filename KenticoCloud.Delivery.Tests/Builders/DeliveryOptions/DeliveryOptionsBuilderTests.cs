using Xunit;

namespace KenticoCloud.Delivery.Tests.Configuration
{
    public class DeliveryOptionsBuilderTests
    {
        private const string ProjectId = "550cec62-90a6-4ab3-b3e4-3d0bb4c04f5c";
        private const string PreviewApiKey = "someTestPreviewApiKey";

        [Fact]
        public void BuildWithProjectIdAndUseProductionApi()
        {
            var deliveryOptions = DeliveryOptionsBuilder
                .CreateInstance()
                .WithProjectId(ProjectId)
                .UseProductionApi
                .Build();

            Assert.Equal(deliveryOptions.ProjectId, ProjectId);
            Assert.False(deliveryOptions.UsePreviewApi);
            Assert.False(deliveryOptions.UseSecuredProductionApi);
        }

        [Fact]
        public void BuildWithProjectIdAndPreviewApi()
        {
            var deliveryOptions = DeliveryOptionsBuilder
                .CreateInstance()
                .WithProjectId(ProjectId)
                .UsePreviewApi(PreviewApiKey)
                .Build();

            Assert.Equal(deliveryOptions.ProjectId, ProjectId);
            Assert.True(deliveryOptions.UsePreviewApi);
        }

        [Fact]
        public void BuildWithProjectIdAndSecuredProductionApi()
        {
            var deliveryOptions = DeliveryOptionsBuilder
                .CreateInstance()
                .WithProjectId(ProjectId)
                .UseSecuredProductionApi(PreviewApiKey)
                .Build();

            Assert.Equal(deliveryOptions.ProjectId, ProjectId);
            Assert.True(deliveryOptions.UseSecuredProductionApi);
        }

        [Fact]
        public void BuildWithMaxRetryAttempts()
        {
            const int maxRetryAttempts = 10;

            var deliveryOptions = DeliveryOptionsBuilder
                .CreateInstance()
                .WithProjectId(ProjectId)
                .UseProductionApi
                .WithMaxRetryAttempts(maxRetryAttempts)
                .Build();

            Assert.Equal(deliveryOptions.MaxRetryAttempts, maxRetryAttempts);
        }

        [Fact]
        public void BuildWithCustomEndpointForPreviewApi()
        {
            const string customEndpoint = "www.customPreviewEndpoint.com";

            var deliveryOptions = DeliveryOptionsBuilder
                .CreateInstance()
                .WithProjectId(ProjectId)
                .UsePreviewApi(PreviewApiKey)
                .WithCustomEndpoint(customEndpoint)
                .Build();

           Assert.Equal(deliveryOptions.PreviewEndpoint, customEndpoint);
        }

        [Fact]
        public void BuildWithCustomEndpointForProductionApi()
        {
            const string customEndpoint = "www.customProductionEndpoint.com";

            var deliveryOptions = DeliveryOptionsBuilder
                .CreateInstance()
                .WithProjectId(ProjectId)
                .UseProductionApi
                .WithCustomEndpoint(customEndpoint)
                .Build();

            Assert.Equal(deliveryOptions.ProductionEndpoint, customEndpoint);
        }
    }
}
