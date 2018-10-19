using System.Net.Http;
using KenticoCloud.Delivery.CodeFirst;
using KenticoCloud.Delivery.ContentLinks;
using KenticoCloud.Delivery.InlineContentItems;
using KenticoCloud.Delivery.ResiliencePolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace KenticoCloud.Delivery
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDeliveryClient(this IServiceCollection services, BuildDeliveryOptions buildDeliveryOptions)
            => services
                .BuildOptions(buildDeliveryOptions)
                .RegisterDependencies();

        public static IServiceCollection AddDeliveryClient(this IServiceCollection services, DeliveryOptions deliveryOptions)
            => services
                .RegisterOptions(deliveryOptions)
                .RegisterDependencies();

        public static IServiceCollection AddDeliveryClient(this IServiceCollection services, IConfiguration configuration, string configurationSectionName = "DeliveryOptions") 
            => services
                .LoadOptionsConfiguration(configuration, configurationSectionName)
                .RegisterDependencies();

        private static IServiceCollection RegisterDependencies(this IServiceCollection services)
        {
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

            return services;
        }

        private static IServiceCollection RegisterOptions(this IServiceCollection services, DeliveryOptions options)
        {
            services.TryAddSingleton<IOptions<DeliveryOptions>>(new OptionsWrapper<DeliveryOptions>(options));

            return services;
        }

        private static IServiceCollection LoadOptionsConfiguration(this IServiceCollection services, IConfiguration configuration, string configurationSectionName)
            => services
                .AddOptions()
                .Configure<DeliveryOptions>(configurationSectionName == null 
                    ? configuration
                    : configuration.GetSection(configurationSectionName));

        private static IServiceCollection BuildOptions(this IServiceCollection services, BuildDeliveryOptions buildDeliveryOptions)
        {
            if (buildDeliveryOptions == null)
                return null;

            var builder = DeliveryOptionsBuilder.CreateInstance();
            var options = buildDeliveryOptions(builder);

            return services.RegisterOptions(options);
        }
    }
}
