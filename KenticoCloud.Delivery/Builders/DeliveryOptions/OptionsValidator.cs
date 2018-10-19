using System;

namespace KenticoCloud.Delivery
{
    public static class OptionsValidator
    {
        public static void Validate(this DeliveryOptions deliveryOptions)
        {
            ValidateMaxRetryAttempts(deliveryOptions.MaxRetryAttempts);
            ValidateProjectId(deliveryOptions.ProjectId);
            ValidateUseOfPreviewAndProductionApi(deliveryOptions);
            IsKeySetForEnabledApi(deliveryOptions);
        }

        internal static void ValidateProjectId(string projectId)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId),
                    "Kentico Cloud project identifier is not specified.");
            }

            if (projectId == string.Empty)
            {
                throw new ArgumentException("Kentico Cloud project identifier is not specified.", nameof(projectId));
            }

            if (!Guid.TryParse(projectId, out var projectIdGuid))
            {
                throw new ArgumentException(
                    "Provided string is not a valid project identifier ({ProjectId}). Haven't you accidentally passed the Preview API key instead of the project identifier?",
                    nameof(projectId));
            }
        }

        internal static void ValidateMaxRetryAttempts(int attempts)
        {
            if (attempts < 0)
            {
                throw new ArgumentException("Number of maximum retry attempts can't be less than zero");
            }
        }

        internal static void IsEmptyOrNull(this string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName, $"Parameter {parameterName} is not specified");
            }

            if (value == string.Empty)
            {
                throw new ArgumentException(parameterName, $"Parameter {parameterName} is not specified");
            }
        }

        internal static void IsKeySetForEnabledApi(DeliveryOptions deliveryOptions)
        {
            if (deliveryOptions.UsePreviewApi && string.IsNullOrEmpty(deliveryOptions.PreviewApiKey))
            {
                throw new InvalidOperationException("The Preview API key must be set while using the Preview API");
            }

            if (deliveryOptions.UseSecuredProductionApi && string.IsNullOrEmpty(deliveryOptions.SecuredProductionApiKey))
            {
                throw new InvalidOperationException("The Secured Production API key must be set while using the Secured Production API");
            }
        }

        internal static void ValidateUseOfPreviewAndProductionApi(this DeliveryOptions deliveryOptions)
        {
            if (deliveryOptions.UsePreviewApi && deliveryOptions.UseSecuredProductionApi)
            {
                throw new InvalidOperationException("Preview API and Secured Production API can't be used at the same time");
            }
        }

        internal static void ValidateCustomEnpoint(this string customEndpoint)
        {
            if (customEndpoint == null)
            {
                throw new ArgumentNullException(nameof(customEndpoint), $"Parameter {nameof(customEndpoint)} is not specified");
            }

            if (customEndpoint == string.Empty)
            {
                throw new ArgumentException(nameof(customEndpoint), $"Parameter {nameof(customEndpoint)} is not specified");
            }

            if (Uri.TryCreate(customEndpoint, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                // throw new ArgumentException(nameof(customEndpoint), $"Parameter {nameof(customEndpoint)} has invalid format");
            }
        }
    }
}
