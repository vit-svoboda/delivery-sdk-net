using System.Net.Http;
using KenticoCloud.Delivery.CodeFirst;
using KenticoCloud.Delivery.ContentLinks;
using KenticoCloud.Delivery.InlineContentItems;
using KenticoCloud.Delivery.ResiliencePolicy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace KenticoCloud.Delivery
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDeliveryClient(this IServiceCollection services, BuildDeliveryOptions buildDeliveryOptions)
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
            
            if (options != null)
            {
                services.TryAddSingleton<IOptions<DeliveryOptions>>(new OptionsWrapper<DeliveryOptions>(options));
            }
            else
            {
                services.AddOptions();
            }

            services.TryAddSingleton<IContentLinkUrlResolver, DefaultContentLinkUrlResolver>();
            services.TryAddSingleton<ICodeFirstTypeProvider, DefaultTypeProvider>();
            services.TryAddSingleton(new HttpClient());
            services.TryAddSingleton<IInlineContentItemsResolver<object>, ReplaceWithWarningAboutRegistrationResolver>();
            services.TryAddSingleton<IInlineContentItemsResolver<UnretrievedContentItem>, ReplaceWithWarningAboutUnretrievedItemResolver>();
            services.TryAddSingleton<IInlineContentItemsProcessor, InlineContentItemsProcessor>();
            services.TryAddSingleton<ICodeFirstModelProvider, CodeFirstModelProvider>();
            services.TryAddSingleton<ICodeFirstPropertyMapper, CodeFirstPropertyMapper>();
            services.TryAddSingleton<IResiliencePolicyProvider, DefaultResiliencePolicyProvider>();
            services.TryAddSingleton<IDeliveryClient, DeliveryClient>();
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
