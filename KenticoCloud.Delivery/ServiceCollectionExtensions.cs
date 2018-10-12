using KenticoCloud.Delivery.Configuration;
using KenticoCloud.Delivery.InlineContentItems;
using KenticoCloud.Delivery.ResiliencePolicy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace KenticoCloud.Delivery
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDeliveryClient(this IServiceCollection services, BuildDeliveryOptions buildDeliveryOptions = null)
        {
            var options = BuildOptions(buildDeliveryOptions);

            return services.AddDeliveryClient(options);
        }
        public static IServiceCollection AddDeliveryClient(this IServiceCollection services, DeliveryOptions deliveryOptions = null)
        {
            RegisterDependencies(services, deliveryOptions);

            return services;
        }

        private static void RegisterDependencies(IServiceCollection services, DeliveryOptions options = null)
        {
            services.AddOptions();
            if (options != null)
            {
                services.TryAddSingleton<IOptions<DeliveryOptions>>(new OptionsWrapper<DeliveryOptions>(options));
            }

            services.TryAddSingleton(serviceProvider => (IContentLinkUrlResolver)null);
            services.TryAddSingleton(serviceProvider => (ICodeFirstTypeProvider)null);
            services.TryAddSingleton<IInlineContentItemsResolver<object>, ReplaceWithWarningAboutRegistrationResolver>();
            services.TryAddSingleton<IInlineContentItemsResolver<UnretrievedContentItem>, ReplaceWithWarningAboutUnretrievedItemResolver>();
            services.TryAddTransient<IInlineContentItemsProcessor, InlineContentItemsProcessor>();
            services.TryAddTransient<ICodeFirstModelProvider, CodeFirstModelProvider>();
            services.TryAddSingleton<ICodeFirstPropertyMapper, CodeFirstPropertyMapper>();
            services.TryAddSingleton<IResiliencePolicyProvider, DefaultResiliencePolicyProvider>();
            services.TryAddTransient<IDeliveryClient, DeliveryClient>();
        }

        private static DeliveryOptions BuildOptions(BuildDeliveryOptions buildDeliveryOptions)
        {
            if (buildDeliveryOptions == null)
                return null;

            var builder = DeliveryOptionsBuilder.CreateInstance();
            return buildDeliveryOptions(builder);
        }
    }
}
