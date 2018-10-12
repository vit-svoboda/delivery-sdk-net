using KenticoCloud.Delivery.Builders.DeliveryOptions;

namespace KenticoCloud.Delivery
{
    public class DeliveryOptionsBuilder : IOptionsPreviewOrProductionSteps, IOptionsMandatorySteps, IOptionsSteps
    {
        private readonly DeliveryOptions _deliveryOptions = new DeliveryOptions();

        public static IOptionsMandatorySteps CreateInstance()
            => new DeliveryOptionsBuilder();

        private DeliveryOptionsBuilder() {}

        IOptionsPreviewOrProductionSteps IOptionsMandatorySteps.WithProjectId(string projectId)
        {
            OptionsValidator.ValidateProjectId(projectId);
            _deliveryOptions.ProjectId = projectId;

            return this;
        }

        IOptionsSteps IOptionsSteps.WaitForLoadingNewContent
        {
            get
            {
                _deliveryOptions.WaitForLoadingNewContent = true;

                return this;
            }
        }

        IOptionsSteps IOptionsSteps.EnableResilienceLogic
        {
            get
            {
                _deliveryOptions.EnableResilienceLogic = true;

                return this;
            }
        }

        IOptionsSteps IOptionsSteps.WithMaxRetryAttempts(int attempts)
        {
            OptionsValidator.ValidateMaxRetryAttempts(attempts);
            _deliveryOptions.MaxRetryAttempts = attempts;

            return this;
        }

        IOptionsSteps IOptionsPreviewOrProductionSteps.UsePreviewApi(string previewApiKey)
        {
            OptionsValidator.ValidateString(previewApiKey, nameof(previewApiKey));
            _deliveryOptions.PreviewApiKey = previewApiKey;
            _deliveryOptions.UsePreviewApi = true;

            return this;
        }
        IOptionsSteps IOptionsPreviewOrProductionSteps.UseProductionApi
            => this;

        IOptionsSteps IOptionsPreviewOrProductionSteps.UseSecuredProductionApi(string securedProductionApiKey)
        {
            OptionsValidator.ValidateString(securedProductionApiKey, nameof(securedProductionApiKey));
            _deliveryOptions.SecuredProductionApiKey = securedProductionApiKey;
            _deliveryOptions.UseSecuredProductionApi = true;

            return this;
        }

        IOptionsSteps IOptionsSteps.WithCustomEndpoint(string endpoint)
        {
            OptionsValidator.ValidateString(endpoint, nameof(endpoint));
            if (_deliveryOptions.UsePreviewApi)
            {
                _deliveryOptions.PreviewEndpoint = endpoint;
            }
            else
            {
                _deliveryOptions.ProductionEndpoint = endpoint;
            }

            return this;
        }

        DeliveryOptions IOptionsBuildStep.Build()
        {
            _deliveryOptions.Validate();

            return _deliveryOptions;
        }
    }
}
