using System.Net.Http;
using KenticoCloud.Delivery.InlineContentItems;
using KenticoCloud.Delivery.ResiliencePolicy;

namespace KenticoCloud.Delivery.Builders.DeliveryClient
{
    /// <summary>
    /// Defines the contract of the last build step that initializes a new instance of the of the <see cref="DeliveryClient"/> class.
    /// </summary>
    public interface IDeliveryClientBuildStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryClient"/> class for retrieving content of the specified project.
        /// </summary>
        IDeliveryClient Build();
    }

    /// <summary>
    /// Defines the contracts of the mandatory steps for building a Kentico Cloud Delivery Client instance.
    /// </summary>
    public interface IDeliveryClientMandatorySteps
    {
        IDeliveryClientOptionalSteps BuildWithProjectId(string projectId);
        IDeliveryClientOptionalSteps BuildWithDeliveryOptions(BuildDeliveryOptions buildDeliveryOptions);
    }

    /// <summary>
    /// Defines the contracts of the optional steps for building a Kentico Cloud Delivery Client instance.
    /// </summary>
    public interface IDeliveryClientOptionalSteps : IDeliveryClientBuildStep
    {
        /// <summary>
        /// Sets a custom HTTP client instance to the Delivery Client instance.
        /// </summary>
        /// <param name="httpClient">A custom HTTP client instance</param>
        /// <returns></returns>
        IDeliveryClientOptionalSteps WithHttpClient(HttpClient httpClient);

        /// <summary>
        /// Sets a custom instance of an object that can resolve links in rich text elements to the Delivery Client instance.
        /// </summary>
        /// <param name="contentLinkUrlResolver">An instance of an object that can resolve links in rich text elements</param>
        /// <returns></returns>
        IDeliveryClientOptionalSteps WithContentLinkUrlResolver(IContentLinkUrlResolver contentLinkUrlResolver);

        /// <summary>
        /// Sets a custom instance of an object that can resolve modular content in rich text elements to the Delivery Client instance.
        /// </summary>
        /// <param name="inlineContentItemsProcessor">An instance of an object that can resolve modular content in rich text elements</param>
        /// <returns></returns>
        IDeliveryClientOptionalSteps WithInlineContentItemsProcessor(IInlineContentItemsProcessor inlineContentItemsProcessor);

        /// <summary>
        /// Sets a custom instance of an object that can JSON responses into strongly typed CLR objects to the Delivery Client instance.
        /// </summary>
        /// <param name="codeFirstModelProvider">An instance of an object that can JSON responses into strongly typed CLR objects</param>
        /// <returns></returns>
        IDeliveryClientOptionalSteps WithCodeFirstModelProvider(ICodeFirstModelProvider codeFirstModelProvider);

        /// <summary>
        /// Sets a custom instance of an object that can map Kentico Cloud content types to CLR types to the Delivery Client instance.
        /// </summary>
        /// <param name="codeFirstTypeProvider">An instance of an object that can map Kentico Cloud content types to CLR types</param>
        /// <returns></returns>
        IDeliveryClientOptionalSteps WithCodeFirstTypeProvider(ICodeFirstTypeProvider codeFirstTypeProvider);

        /// <summary>
        /// Sets a custom instance of a provider of a resilience (retry) policy to the Delivery Client instance.
        /// </summary>
        /// <param name="resiliencePolicyProvider">A provider of a resilience (retry) policy</param>
        /// <returns></returns>
        IDeliveryClientOptionalSteps WithResiliencePolicyProvider(IResiliencePolicyProvider resiliencePolicyProvider);

        /// <summary>
        /// Sets a custom instance of an object that can map Kentico Cloud content item fields to model properties to the Delivery Client instance.
        /// </summary>
        /// <param name="propertyMapper">An instance of an object that can map Kentico Cloud content item fields to model properties</param>
        /// <returns></returns>
        IDeliveryClientOptionalSteps WithCodeFirstPropertyMapper(ICodeFirstPropertyMapper propertyMapper);
    }
}
