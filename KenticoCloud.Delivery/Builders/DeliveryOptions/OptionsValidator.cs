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

        internal static void ValidateString(this string value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(parameterName, $"Parameter {parameterName} is not specified");
            }
        }

        internal static void ValidateUseOfPreviewAndProductionApi(this DeliveryOptions deliveryOptions)
        {
            if (deliveryOptions.UsePreviewApi && deliveryOptions.UseSecuredProductionApi)
            {
                throw new InvalidOperationException("Preview API and Secured Production API can't be used at the same time");
            }
        }
    }
}
