using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using FakeItEasy;
using KenticoCloud.Delivery.InlineContentItems;
using Microsoft.Extensions.Options;
using Polly;
using RichardSzalay.MockHttp;
using Xunit;

namespace KenticoCloud.Delivery.Rx.Tests
{
    public class DeliveryObservableProxyTests
    {
        private const string BEVERAGES_IDENTIFIER = "coffee_beverages_explained";
        readonly string guid = string.Empty;
        readonly string baseUrl = string.Empty;
        readonly MockHttpMessageHandler mockHttp;

        public DeliveryObservableProxyTests()
        {
            guid = Guid.NewGuid().ToString();
            baseUrl = $"https://deliver.kenticocloud.com/{guid}";
            mockHttp = new MockHttpMessageHandler();
        }

        [Fact]
        public async void ItemJsonRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockItem)).GetItemJsonObservable(BEVERAGES_IDENTIFIER, "language=es-ES");
            var itemJson = await observable.FirstOrDefaultAsync();

            Assert.Single(observable.ToEnumerable());
            Assert.NotNull(itemJson);
        }

        [Fact]
        public void ItemsJsonRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockItems)).GetItemsJsonObservable("limit=2", "skip=1");
            var itemsJson = observable.ToEnumerable().ToList();

            Assert.NotEmpty(itemsJson);
            Assert.Equal(2, itemsJson[0]["items"].Count());
        }

        [Fact]
        public async void ContentItemRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockItem)).GetItemObservable(BEVERAGES_IDENTIFIER, new LanguageParameter("es-ES"));
            var item = await observable.FirstOrDefaultAsync();

            Assert.NotNull(item);
            AssertItemPropertiesNotNull(item);
        }

        [Fact]
        public async void TypedItemRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockItem)).GetItemObservable<Article>(BEVERAGES_IDENTIFIER, new LanguageParameter("es-ES"));
            var item = await observable.FirstOrDefaultAsync();

            Assert.NotNull(item);
            AssertArticlePropertiesNotNull(item);
        }

        [Fact]
        public async void RuntimeTypedItemRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockItem)).GetItemObservable<object>(BEVERAGES_IDENTIFIER, new LanguageParameter("es-ES"));
            var item = await observable.FirstOrDefaultAsync();

            Assert.IsType<Article>(item);
            Assert.NotNull(item);
            AssertArticlePropertiesNotNull((Article)item);
        }

        [Fact]
        public void ContentItemsRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockItems)).GetItemsObservable(new LimitParameter(2), new SkipParameter(1));
            var items = observable.ToEnumerable().ToList();

            Assert.NotEmpty(items);
            Assert.Equal(2, items.Count);
            Assert.All(items, item => AssertItemPropertiesNotNull(item));
        }

        [Fact]
        public void TypedItemsRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockArticles)).GetItemsObservable<Article>(new ContainsFilter("elements.personas", "barista"));
            var items = observable.ToEnumerable().ToList();

            Assert.NotEmpty(items);
            Assert.Equal(6, items.Count);
            Assert.All(items, article => AssertArticlePropertiesNotNull(article));
        }

        [Fact]
        public void RuntimeTypedItemsRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockArticles)).GetItemsObservable<Article>(new ContainsFilter("elements.personas", "barista"));
            var articles = observable.ToEnumerable().ToList();

            Assert.NotEmpty(articles);
            Assert.All(articles, article => Assert.IsType<Article>(article));
            Assert.All(articles, article => AssertArticlePropertiesNotNull(article));
        }

        [Fact]
        public async void TypeJsonRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockType)).GetTypeJsonObservable(Article.Codename);
            var type = await observable.FirstOrDefaultAsync();

            Assert.Single(observable.ToEnumerable());
            Assert.NotNull(type);
        }

        [Fact]
        public void TypesJsonRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockTypes)).GetTypesJsonObservable("skip=2");
            var types = observable.ToEnumerable().ToList();

            Assert.NotEmpty(types);
            Assert.Equal(13, types[0]["types"].Count());
        }

        [Fact]
        public async void TypeRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockType)).GetTypeObservable(Article.Codename);
            var type = await observable.FirstOrDefaultAsync();

            Assert.Single(observable.ToEnumerable());
            Assert.NotNull(type.System);
            Assert.NotEmpty(type.Elements);
        }

        [Fact]
        public void TypesRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockTypes)).GetTypesObservable(new SkipParameter(2));
            var types = observable.ToEnumerable().ToList();

            Assert.NotEmpty(types);
            Assert.All(types, type => Assert.NotNull(type));
            Assert.All(types, type => Assert.NotEmpty(type.Elements));
        }

        [Fact]
        public async void ElementRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockElement)).GetElementObservable(Article.Codename, Article.TitleCodename);
            var element = await observable.FirstOrDefaultAsync();

            Assert.NotNull(element);
            Assert.NotNull(element.Codename);
            Assert.NotNull(element.Name);
            Assert.NotNull(element.Options);
            Assert.NotNull(element.TaxonomyGroup);
            Assert.NotNull(element.Type);
        }

        [Fact]
        public async void TaxonomyJsonRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockTaxonomy)).GetTaxonomyJsonObservable("personas");
            var taxonomyJson = await observable.FirstOrDefaultAsync();

            Assert.NotNull(taxonomyJson);
            Assert.NotNull(taxonomyJson["system"]);
            Assert.NotNull(taxonomyJson["terms"]);
        }

        [Fact]
        public void TaxonomiesJsonRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockTaxonomies)).GetTaxonomiesJsonObservable("skip=1");
            var taxonomiesJson = observable.ToEnumerable().ToList();

            Assert.NotNull(taxonomiesJson);
            Assert.NotNull(taxonomiesJson[0]["taxonomies"]);
            Assert.NotNull(taxonomiesJson[0]["pagination"]);
        }

        [Fact]
        public async void TaxonomyRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockTaxonomy)).GetTaxonomyObservable("personas");
            var taxonomy = await observable.FirstOrDefaultAsync();

            Assert.NotNull(taxonomy);
            Assert.NotNull(taxonomy.System);
            Assert.NotNull(taxonomy.Terms);
        }

        [Fact]
        public void TaxonomiesRetrieved()
        {
            var observable = new DeliveryObservableProxy(GetDeliveryClient(MockTaxonomies)).GetTaxonomiesObservable(new SkipParameter(1));
            var taxonomies = observable.ToEnumerable().ToList();

            Assert.NotEmpty(taxonomies);
            Assert.All(taxonomies, taxonomy => Assert.NotNull(taxonomy.System));
            Assert.All(taxonomies, taxonomy => Assert.NotNull(taxonomy.Terms));
        }

        private IDeliveryClient GetDeliveryClient(Action mockAction)
        {
            mockAction();
            var httpClient = mockHttp.ToHttpClient();
            var deliveryOptions = new OptionsWrapper<DeliveryOptions>(new DeliveryOptions { ProjectId = guid });
            var contentLinkUrlResolver = A.Fake<IContentLinkUrlResolver>();
            var contentItemsProcessor = A.Fake<IInlineContentItemsProcessor>();
            var contentPropertyMapper =  new CodeFirstPropertyMapper();
            var contentTypeProvider = new CustomTypeProvider();
            var codeFirstModelProvider = new CodeFirstModelProvider(contentLinkUrlResolver, contentItemsProcessor, contentTypeProvider, contentPropertyMapper);
            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(deliveryOptions.Value.MaxRetryAttempts));
            var client = new DeliveryClient(
                deliveryOptions, 
                contentLinkUrlResolver, 
                null,
                codeFirstModelProvider,
                null,
                contentTypeProvider
            )
            {
                HttpClient = httpClient,
            };

            return client;
        }

        private void MockItem()
        {
            mockHttp.When($"{baseUrl}/items/{BEVERAGES_IDENTIFIER}?language=es-ES")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\coffee_beverages_explained.json")));
        }

        private void MockItems()
        {
            mockHttp.When($"{baseUrl}/items")
                .WithQueryString("limit=2&skip=1")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\items.json")));
        }

        private void MockArticles()
        {
            mockHttp.When($"{baseUrl}/items")
                .WithQueryString(new[] { new KeyValuePair<string, string>("system.type", Article.Codename), new KeyValuePair<string, string>("elements.personas[contains]", "barista") })
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\articles.json")));
        }

        private void MockType()
        {
            mockHttp.When($"{baseUrl}/types/{Article.Codename}")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\article-type.json")));
        }

        private void MockTypes()
        {
            mockHttp.When($"{baseUrl}/types?skip=2")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\types.json")));
        }

        private void MockElement()
        {
            mockHttp.When($"{baseUrl}/types/{Article.Codename}/elements/{Article.TitleCodename}")
                .Respond("application/json", "{'type':'text','name':'Title','codename':'title'}");
        }

        private void MockTaxonomy()
        {
            mockHttp.When($"{baseUrl}/taxonomies/personas")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\taxonomies_personas.json")));
        }

        private void MockTaxonomies()
        {
            mockHttp.When($"{baseUrl}/taxonomies")
                .WithQueryString("skip=1")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\taxonomies_multiple.json")));
        }

        private static void AssertItemPropertiesNotNull(ContentItem item)
        {
            Assert.NotNull(item.System);
            Assert.NotNull(item.Elements);
        }

        private static void AssertArticlePropertiesNotNull(Article item)
        {
            Assert.NotNull(item.System);
            Assert.NotNull(item.Personas);
            Assert.NotNull(item.Title);
            Assert.NotNull(item.TeaserImage);
            Assert.NotNull(item.PostDate);
            Assert.NotNull(item.Summary);
            Assert.NotNull(item.BodyCopy);
            Assert.NotNull(item.RelatedArticles);
            Assert.NotNull(item.MetaKeywords);
            Assert.NotNull(item.MetaDescription);
            Assert.NotNull(item.UrlPattern);
        }
    }
}
