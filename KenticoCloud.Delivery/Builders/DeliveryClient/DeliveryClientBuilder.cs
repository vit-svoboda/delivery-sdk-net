using System;
using System.Net.Http;
using KenticoCloud.Delivery.Builders.DeliveryClient;
using KenticoCloud.Delivery.Builders.DeliveryOptions;
using KenticoCloud.Delivery.InlineContentItems;
using KenticoCloud.Delivery.ResiliencePolicy;
using Microsoft.Extensions.DependencyInjection;

namespace KenticoCloud.Delivery
{
    public delegate DeliveryOptions BuildDeliveryOptions(IOptionsMandatorySteps builder);

    public sealed class DeliveryClientBuilder : IDeliveryClientMandatorySteps, IDeliveryClientOptionalSteps
    {
        private static IDeliveryClientMandatorySteps MandatorySteps => new DeliveryClientBuilder();

        /// <summary>
        /// Mandatory step of the <see cref="DeliveryClientBuilder"/> for specifying Kentico Cloud project id.
        /// </summary>
        /// <param name="projectId">The identifier of the Kentico Cloud project.</param>
        /// <returns></returns>
        public static IDeliveryClientOptionalSteps WithProjectId(string projectId)
        {
            OptionsValidator.ValidateProjectId(projectId);
            
            return MandatorySteps.BuildWithProjectId(projectId);
        }

        /// <summary>
        /// Mandatory step of the <see cref="DeliveryClientBuilder"/> for specifying Kentico Cloud project settings.
        /// </summary>
        /// <param name="buildDeliveryOptions">A function that returns <see cref="DeliveryOptions"/> instance which specifies the Kentico Cloud project settings.</param>
        /// <returns></returns>
        public static IDeliveryClientOptionalSteps WithOptions(BuildDeliveryOptions buildDeliveryOptions)
            => MandatorySteps.BuildWithDeliveryOptions(buildDeliveryOptions);

        private readonly IServiceCollection _serviceCollection;
        private HttpClient _httpClient;
        private DeliveryOptions _deliveryOptions;

        private DeliveryClientBuilder()
        {
            _serviceCollection = new ServiceCollection();
        }

        IDeliveryClientOptionalSteps IDeliveryClientMandatorySteps.BuildWithProjectId(string projectId)
            => BuildWithDeliveryOptions(builder => builder.WithProjectId(projectId).UseProductionApi.Build());

        public IDeliveryClientOptionalSteps BuildWithDeliveryOptions(BuildDeliveryOptions buildDeliveryOptions)
        {
            var builder = DeliveryOptionsBuilder.CreateInstance();            

            _deliveryOptions = buildDeliveryOptions(builder);

            return this;
        }

        IDeliveryClientOptionalSteps IDeliveryClientOptionalSteps.WithHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient), "Http client is not specified");

            _serviceCollection.AddSingleton(_httpClient);

            return this;
        }

        IDeliveryClientOptionalSteps IDeliveryClientOptionalSteps.WithContentLinkUrlResolver(IContentLinkUrlResolver contentLinkUrlResolver)
        {
            if (contentLinkUrlResolver == null)
            {
                throw new ArgumentNullException(nameof(contentLinkUrlResolver));
            }

            _serviceCollection.AddSingleton(contentLinkUrlResolver);

            return this;
        }

        IDeliveryClientOptionalSteps IDeliveryClientOptionalSteps.WithInlineContentItemsProcessor(IInlineContentItemsProcessor inlineContentItemsProcessor)
        {
            if (inlineContentItemsProcessor == null)
            {
                throw new ArgumentNullException(nameof(inlineContentItemsProcessor));
            }

            _serviceCollection.AddSingleton(inlineContentItemsProcessor);

            return this;
        }

        IDeliveryClientOptionalSteps IDeliveryClientOptionalSteps.WithCodeFirstModelProvider(ICodeFirstModelProvider codeFirstModelProvider)
        {
            if (codeFirstModelProvider == null)
            {
                throw new ArgumentNullException(nameof(codeFirstModelProvider));
            }

            _serviceCollection.AddSingleton(codeFirstModelProvider);

            return this;
        }

        IDeliveryClientOptionalSteps IDeliveryClientOptionalSteps.WithCodeFirstTypeProvider(ICodeFirstTypeProvider codeFirstTypeProvider)
        {
            if (codeFirstTypeProvider == null)
            {
                throw new ArgumentNullException(nameof(codeFirstTypeProvider));
            }

            _serviceCollection.AddSingleton(codeFirstTypeProvider);

            return this;
        }

        IDeliveryClientOptionalSteps IDeliveryClientOptionalSteps.WithResiliencePolicyProvider(IResiliencePolicyProvider resiliencePolicyProvider)
        {
            if (resiliencePolicyProvider == null)
            {
                throw new ArgumentNullException(nameof(resiliencePolicyProvider));
            }

            _serviceCollection.AddSingleton(resiliencePolicyProvider);

            return this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryClient"/> class for retrieving content of the specified project.
        /// </summary>
        /// <param name="propertyMapper">An instance of an object that can map Kentico Cloud content item fields to model properties</param>
        IDeliveryClientOptionalSteps IDeliveryClientOptionalSteps.WithCodeFirstPropertyMapper(ICodeFirstPropertyMapper propertyMapper)
        {
            if (propertyMapper == null)
            {
                throw new ArgumentNullException(nameof(propertyMapper));
            }

            _serviceCollection.AddSingleton(propertyMapper);

            return this;
        }

        IDeliveryClient IDeliveryClientBuildStep.Build()
        {
            _serviceCollection.AddDeliveryClient(_deliveryOptions);

            var serviceProvider = _serviceCollection.BuildServiceProvider();

            var client = serviceProvider.GetService<IDeliveryClient>();

            return client;
        }
    }
}
