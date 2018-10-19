﻿namespace KenticoCloud.Delivery
{
    /// <summary>
    /// Keeps settings which are provided by customer or have default values, used in <see cref="DeliveryClient"/>.
    /// </summary>
    public class DeliveryOptions
    {
        /// <summary>
        /// Gets or sets the Production endpoint address.
        /// </summary>
        public string ProductionEndpoint { get; set; } = "https://deliver.kenticocloud.com/{0}";

        /// <summary>
        /// Gets or sets the Preview endpoint address.
        /// </summary>
        public string PreviewEndpoint { get; set; } = "https://preview-deliver.kenticocloud.com/{0}";

        /// <summary>
        /// Gets or sets the Project identifier.
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the Preview API key.
        /// </summary>
        public string PreviewApiKey { get; set; }

        // this property enables user to have preview API key set and not use it
        /// <summary>
        /// Gets or sets whether the Preview API should be used. If TRUE, <see cref="PreviewApiKey"/> needs to be set as well.
        /// </summary>
        public bool UsePreviewApi { get; set; }

        /// <summary>
        /// Set to true if you want to wait for updated content. It should be used when you are acting upon a webhook call.
        /// </summary>
        public bool WaitForLoadingNewContent { get; set; }

        // this property enables user to have secured production API key set and not use it
        /// <summary>
        /// Gets or sets whether the production Delivery API will be accessed using an API key.
        /// </summary>
        public bool UseSecuredProductionApi { get; set; }

        /// <summary>
        /// Gets or sets the production Delivery API key.
        /// </summary>
        public string SecuredProductionApiKey { get; set; }

        /// <summary>
        /// Gets or sets whether HTTP requests will use a retry logic.
        /// </summary>
        public bool EnableResilienceLogic { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum retry attempts.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 5;
    }
}
