using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace KenticoCloud.Delivery.Tests.Extensions
{
    internal class FakeServiceCollection : IServiceCollection
    {
        internal Dictionary<Type, ImplementationAndLifetime> Dependencies =
            new Dictionary<Type, ImplementationAndLifetime>();

        public IEnumerator<ServiceDescriptor> GetEnumerator()
            => Enumerable.Empty<ServiceDescriptor>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ServiceDescriptor item)
        {
            Dependencies.Add(item.ServiceType, new ImplementationAndLifetime{Implementation = item.ImplementationType, Lifetime = item.Lifetime});
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
        public int IndexOf(ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public ServiceDescriptor this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }

    internal struct ImplementationAndLifetime
    {
        internal object Implementation;
        internal ServiceLifetime Lifetime;
    }

    internal static class FakeServiceCollectionExtensions
    {
        public static void TryAddSingleton(this FakeServiceCollection serviceCollection, object implementation) 
            => serviceCollection.TryAddSingleton(implementation);

        public static void TryAddSingleton<TType, TImplementation>(this FakeServiceCollection serviceCollection) 
            => serviceCollection.TryAddSingleton<TType, TImplementation>();
    }
}
