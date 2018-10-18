using KenticoCloud.Delivery.Tests.DIFrameworks.Helpers;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector.Integration.AspNetCore;
using SimpleInjector;
using Xunit;
using SimpleInjector.Lifestyles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;

namespace KenticoCloud.Delivery.Tests.DIFrameworks
{
    public class SimpleInjectorTests
    {
        [Fact]
        public void DeliveryClientIsSuccessfullyResolvedFromSimpleInjectorContainer()
        {
            var serviceCollection = DIFrameworksHelper.GetServiceCollection();
            var container = new Container();

            var appBuilder = new FakeApplicationBuilder
            {
                ApplicationServices = serviceCollection.BuildServiceProvider()
            };

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            serviceCollection.EnableSimpleInjectorCrossWiring(container);
            serviceCollection.UseSimpleInjectorAspNetRequestScoping(container);
            container.AutoCrossWireAspNetComponents(appBuilder);
            
            var client = (DeliveryClient) container.GetInstance<IDeliveryClient>();

            DIFrameworksHelper.AssertDefaultDeliveryClient(client);
        }

        [Fact]
        public void DeliveryClientIsSuccessfullyResolvedFromSimpleInjectorContainer_CustomCodeFirstModelProvider()
        {
            var container = new Container();
            var modelProvider = new FakeModelProvider();
            var serviceCollection = DIFrameworksHelper.GetServiceCollection();

            serviceCollection.AddSingleton<ICodeFirstModelProvider>(_ => modelProvider);

            var appBuilder = new FakeApplicationBuilder
            {
                ApplicationServices = serviceCollection.BuildServiceProvider()
            };

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            serviceCollection.EnableSimpleInjectorCrossWiring(container);
            serviceCollection.UseSimpleInjectorAspNetRequestScoping(container);
            container.AutoCrossWireAspNetComponents(appBuilder);

            var client = (DeliveryClient)container.GetInstance<IDeliveryClient>();

            DIFrameworksHelper.AssertDeliveryClientWithCustomCodeFirstModelProvider(client, modelProvider);
        }
    }

    class FakeApplicationBuilder : IApplicationBuilder
    {
        public IServiceProvider ApplicationServices { get; set; }

        public IFeatureCollection ServerFeatures => throw new NotImplementedException();

        public IDictionary<String, Object> Properties => throw new NotImplementedException();

        public RequestDelegate Build() => throw new NotImplementedException();
        public IApplicationBuilder New() => throw new NotImplementedException();
        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware) => throw new NotImplementedException();
    }
}
