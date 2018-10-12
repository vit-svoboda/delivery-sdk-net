namespace KenticoCloud.Delivery.Builders.DeliveryOptions
{
    /// <summary>
    /// Defines the contracts of the mandatory steps for building a <see cref="DeliveryOptions"/> instance.
    /// </summary>
    public interface IOptionsMandatorySteps
    {
        /// <summary>
        /// A mandatory step of the <see cref="DeliveryOptionsBuilder"/> for specifying Kentico Cloud project id.
        /// </summary>
        /// <param name="projectId">The identifier of the Kentico Cloud project.</param>
        /// <returns></returns>
        IOptionsPreviewOrProductionSteps WithProjectId(string projectId);
    }

    /// <summary>
    /// Defines the contracts of the optional steps for building a <see cref="DeliveryOptions"/> instance.
    /// </summary>
    public interface IOptionsSteps : IOptionsBuildStep
    {
        /// <summary>
        /// An optional step that sets that HTTP requests will use a retry logic.
        /// </summary>
        IOptionsSteps EnableResilienceLogic { get; }

        /// <summary>
        /// An optional step that sets the client to wait for updated content. It should be used when you are acting upon a webhook call.
        /// </summary>
        IOptionsSteps WaitForLoadingNewContent { get; }

        /// <summary>
        /// An optional step that sets the maximum number of retry attempts.
        /// </summary>
        /// <param name="attempts">Maximum retry attempts</param>
        /// <returns></returns>
        IOptionsSteps WithMaxRetryAttempts(int attempts);

        /// <summary>
        /// An optional step that sets a custom endpoint for a chosen API.
        /// </summary>
        /// <param name="customEndpoint">A custom endpoint address.</param>
        /// <returns></returns>
        IOptionsSteps WithCustomEndpoint(string customEndpoint);
    }

    /// <summary>
    /// Defines the contracts of different APIs that might be used.
    /// </summary>
    public interface IOptionsPreviewOrProductionSteps
    {
        /// <summary>
        /// Sets the Delivery Client to make requests to a Production API.
        /// </summary>
        IOptionsSteps UseProductionApi { get; }

        /// <summary>
        /// Sets the Delivery Client to make requests to a Preview API.
        /// </summary>
        /// <param name="previewApiKey">A Preview API key</param>
        /// <returns></returns>
        IOptionsSteps UsePreviewApi(string previewApiKey);

        /// <summary>
        /// Sets the Delivery Client to make requests to a Secured Production API.
        /// </summary>
        /// <param name="securedProductionApiKey"></param>
        /// <returns></returns>
        IOptionsSteps UseSecuredProductionApi(string securedProductionApiKey);
    }

    /// <summary>
    /// Defines the contract of the last build step that initializes a new instance of the of the <see cref="DeliveryOptions"/> class.
    /// </summary>
    public interface IOptionsBuildStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryOptions" /> class that configures the Kentico Delivery Client.
        /// </summary>
        /// <returns><see cref="DeliveryOptions"/> instance</returns>
        Delivery.DeliveryOptions Build();
    }
}
