﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using KenticoCloud.Delivery.ResiliencePolicy;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Xunit;
using RichardSzalay.MockHttp;
using Polly;

namespace KenticoCloud.Delivery.Tests
{
    public class DeliveryClientTests
    {
        private readonly string _guid;
        private readonly string _baseUrl;
        private readonly MockHttpMessageHandler _mockHttp;

        public DeliveryClientTests()
        {
            _guid = new Guid().ToString();
            _baseUrl = $"https://deliver.kenticocloud.com/{_guid}";
            _mockHttp = new MockHttpMessageHandler();
        }

        [Fact]
        public async void GetItemAsync()
        {
            string url = $"{_baseUrl}/items/";

            _mockHttp.When($"{url}{"coffee_beverages_explained"}")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\coffee_beverages_explained.json")));

            _mockHttp.When($"{url}{"brazil_natural_barra_grande"}")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\brazil_natural_barra_grande.json")));

            _mockHttp.When($"{url}{"on_roasts"}").
            Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\on_roasts.json")));

            var httpClient = _mockHttp.ToHttpClient();
            var contentLinkUrlResolver = A.Fake<IContentLinkUrlResolver>();
            var deliveryOptions = new OptionsWrapper<DeliveryOptions>(new DeliveryOptions { ProjectId = _guid });
            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(deliveryOptions.Value.MaxRetryAttempts));
            var codeFirstModelProvider = new CodeFirstModelProvider(contentLinkUrlResolver, null, null, new CodeFirstPropertyMapper());
            var client = new DeliveryClient(
                deliveryOptions,
                httpClient,
                contentLinkUrlResolver,
                null,
                codeFirstModelProvider,
                resiliencePolicyProvider
            );

            var beveragesResponse = (await client.GetItemAsync("coffee_beverages_explained"));
            var beveragesItem = beveragesResponse.Item;
            var barraItem = (await client.GetItemAsync("brazil_natural_barra_grande")).Item;
            var roastsItem = (await client.GetItemAsync("on_roasts")).Item;
            Assert.Equal("article", beveragesItem.System.Type);
            Assert.Equal("en-US", beveragesItem.System.Language);
            Assert.NotEmpty(beveragesItem.System.SitemapLocation);
            Assert.NotEmpty(roastsItem.GetModularContent("related_articles"));
            Assert.Equal(beveragesItem.Elements.title.value.ToString(), beveragesItem.GetString("title"));
            Assert.Equal(beveragesItem.Elements.body_copy.value.ToString(), beveragesItem.GetString("body_copy"));
            Assert.Equal(DateTime.Parse(beveragesItem.Elements.post_date.value.ToString()), beveragesItem.GetDateTime("post_date"));
            Assert.Equal(beveragesItem.Elements.teaser_image.value.Count, beveragesItem.GetAssets("teaser_image").Count());
            Assert.Equal(beveragesItem.Elements.personas.value.Count, beveragesItem.GetTaxonomyTerms("personas").Count());
            Assert.Equal(decimal.Parse(barraItem.Elements.price.value.ToString()), barraItem.GetNumber("price"));
            Assert.Equal(barraItem.Elements.processing.value.Count, barraItem.GetOptions("processing").Count());
            Assert.NotNull(beveragesResponse.ApiUrl);
        }

        [Fact]
        public async void GetPagination()
        {
            _mockHttp.When($"{_baseUrl}/items")
                .WithQueryString("limit=2&skip=1")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\articles.json")));

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            var articles = await client.GetItemsAsync(new LimitParameter(2), new SkipParameter(1));

            Assert.Equal(2, articles.Pagination.Count);
            Assert.Equal(1, articles.Pagination.Skip);
            Assert.Equal(2, articles.Pagination.Limit);
            Assert.NotNull(articles.Pagination.NextPageUrl);
        }

        [Fact]
        public async void AssetPropertiesNotEmpty()
        {
            _mockHttp.When($"{_baseUrl}/items/{"coffee_beverages_explained"}")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\coffee_beverages_explained.json")));

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            var response = await client.GetItemAsync("coffee_beverages_explained");
            var beveragesItem = response.Item;

            var model = beveragesItem.CastTo<Article>();

            Assert.NotNull(model.TeaserImage.FirstOrDefault()?.Description);
            Assert.NotNull(model.TeaserImage.FirstOrDefault()?.Name);
            Assert.NotNull(model.TeaserImage.FirstOrDefault()?.Type);
            Assert.NotNull(model.TeaserImage.FirstOrDefault()?.Url);
            Assert.NotNull(response.ApiUrl);
        }

        [Fact]
        public async void IgnoredSerializationProperties()
        {
            _mockHttp.When($"{_baseUrl}/items/{"coffee_beverages_explained"}")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\coffee_beverages_explained.json")));

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            var beveragesItem = (await client.GetItemAsync("coffee_beverages_explained")).Item;

            var model = beveragesItem.CastTo<Article>();

            Assert.NotNull(model.TitleNotIgnored);
            Assert.Null(model.TitleIgnored);
        }

        [Fact]
        public async void GetItemAsync_NotFound()
        {
            string messsge = "{'message': 'The requested content item unscintillating_hemerocallidaceae_des_iroquois was not found.','request_id': '','error_code': 101,'specific_code': 0}";

            _mockHttp.When($"{_baseUrl}/items/unscintillating_hemerocallidaceae_des_iroquois")
                .Respond(HttpStatusCode.NotFound, "application/json", messsge);

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            await Assert.ThrowsAsync<DeliveryException>(async () => await client.GetItemAsync("unscintillating_hemerocallidaceae_des_iroquois"));
        }

        [Fact]
        public async void GetItemsAsyncWithTypeExtractor()
        {
            _mockHttp.When($"{_baseUrl}/items")
                .WithQueryString("system.type=cafe")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\allendale.json")));

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            var response = await client.GetItemsAsync<Cafe>();

            Assert.NotEmpty(response.Items);
        }

        [Fact]
        public async void GetItemsAsync()
        {
            _mockHttp.When($"{_baseUrl}/items")
                .WithQueryString("system.type=cafe")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\allendale.json")));

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            var response = await client.GetItemsAsync(new EqualsFilter("system.type", "cafe"));

            Assert.NotEmpty(response.Items);
        }

        [Fact]
        public async void GetTypeAsync()
        {
            _mockHttp.When($"{_baseUrl}/types/article")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\article.json")));

            _mockHttp.When($"{_baseUrl}/types/coffee")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\coffee.json")));

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            var articleType = await client.GetTypeAsync("article");
            var coffeeType = await client.GetTypeAsync("coffee");

            var taxonomyElement = articleType.Elements["personas"];
            var processingTaxonomyElement = coffeeType.Elements["processing"];

            Assert.Equal("article", articleType.System.Codename);
            Assert.Equal("text", articleType.Elements["title"].Type);
            Assert.Equal("rich_text", articleType.Elements["body_copy"].Type);
            Assert.Equal("date_time", articleType.Elements["post_date"].Type);
            Assert.Equal("asset", articleType.Elements["teaser_image"].Type);
            Assert.Equal("modular_content", articleType.Elements["related_articles"].Type);
            Assert.Equal("taxonomy", articleType.Elements["personas"].Type);

            Assert.Equal("number", coffeeType.Elements["price"].Type);
            Assert.Equal("taxonomy", coffeeType.Elements["processing"].Type);

            Assert.Equal("personas", taxonomyElement.TaxonomyGroup);
            Assert.Equal("processing", processingTaxonomyElement.TaxonomyGroup);
        }

        [Fact]
        public async void GetTypeAsync_NotFound()
        {
            string messsge = "{'message': 'The requested content type unequestrian_nonadjournment_sur_achoerodus was not found','request_id': '','error_code': 101,'specific_code': 0}";

            _mockHttp.When($"{_baseUrl}/types/unequestrian_nonadjournment_sur_achoerodus")
                .Respond(HttpStatusCode.NotFound, "application/json", messsge);

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            await Assert.ThrowsAsync<DeliveryException>(async () => await client.GetTypeAsync("unequestrian_nonadjournment_sur_achoerodus"));
        }

        [Fact]
        public async void GetTypesAsync()
        {
            _mockHttp.When($"{_baseUrl}/types")
                .WithQueryString("skip=1")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\types_accessory.json")));

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            var response = await client.GetTypesAsync(new SkipParameter(1));

            Assert.NotNull(response.ApiUrl);
            Assert.NotEmpty(response.Types);
        }

        [Fact]
        public async void GetContentElementAsync()
        {
            string url = $"{_baseUrl}/types";

            _mockHttp.When($"{url}/{Article.Codename}/elements/{Article.TitleCodename}")
                .Respond("application/json", "{'type':'text','name':'Title','codename':'title'}");
            _mockHttp.When($"{url}/{Article.Codename}/elements/{Article.PersonasCodename}")
                .Respond("application/json", "{'type':'taxonomy','name':'Personas','codename':'Personas','taxonomy_group':'personas'}");
            _mockHttp.When($"{url}/{Coffee.Codename}/elements/{Coffee.ProcessingCodename}")
                .Respond("application/json", "{'type':'taxonomy','name':'Processing','taxonomy_group':'processing','codename':'processing'}");

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            var element = await client.GetContentElementAsync(Article.Codename, Article.TitleCodename);
            var personasTaxonomyElement = await client.GetContentElementAsync(Article.Codename, Article.PersonasCodename);
            var processingTaxonomyElement = await client.GetContentElementAsync(Coffee.Codename, Coffee.ProcessingCodename);

            Assert.Equal(Article.TitleCodename, element.Codename);
            Assert.Equal(Article.PersonasCodename, personasTaxonomyElement.TaxonomyGroup);
            Assert.Equal(Coffee.ProcessingCodename, processingTaxonomyElement.TaxonomyGroup);
        }

        [Fact]
        public async void GetContentElementsAsync_NotFound()
        {
            string url = $"{_baseUrl}/types/anticommunistical_preventure_sur_helxine/elements/unlacerated_topognosis_sur_nonvigilantness";

            string messsge = "{'message': 'The requested content type anticommunistical_preventure_sur_helxine was not found.','request_id': '','error_code': 101,'specific_code': 0}";
            _mockHttp.When($"{url}")
                .Respond(HttpStatusCode.NotFound, "application/json", messsge);

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            await Assert.ThrowsAsync<DeliveryException>(async () => await client.GetContentElementAsync("anticommunistical_preventure_sur_helxine", "unlacerated_topognosis_sur_nonvigilantness"));
        }

        [Fact]
        public async void GetTaxonomyAsync()
        {
            _mockHttp.When($"{_baseUrl}/taxonomies/personas")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\taxonomies_personas.json")));

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            var taxonomy = await client.GetTaxonomyAsync("personas");
            var personasTerms = taxonomy.Terms.ToList();
            var coffeeExpertTerms = personasTerms[0].Terms.ToList();

            Assert.Equal("personas", taxonomy.System.Codename);
            Assert.Equal("Personas", taxonomy.System.Name);
            Assert.Equal("coffee_expert", personasTerms[0].Codename);
            Assert.Equal("Coffee expert", personasTerms[0].Name);
            Assert.Equal("cafe_owner", coffeeExpertTerms[1].Codename);
            Assert.Equal("Cafe owner", coffeeExpertTerms[1].Name);
        }

        //[Fact]
        //public async void GetTaxonomyAsync_NotFound()
        //{
        //    string url = $"{_baseUrl}/taxonomies/unequestrian_nonadjournment_sur_achoerodus";
        //    _mockHttp.When($"{url}")
        //        .Respond(HttpStatusCode.NotFound, "application/json", "{'message':'The requested taxonomy group unequestrian_nonadjournment_sur_achoerodus was not found.'}");

        //    var httpClient = _mockHttp.ToHttpClient();

        //    var client = new DeliveryClient(_guid)
        //    {
        //        // CodeFirstModelProvider = { TypeProvider = new CustomTypeProvider() },
        //        HttpClient = httpClient
        //    };

        //    await Assert.ThrowsAsync<DeliveryException>(async () => await client.GetTaxonomyAsync("unequestrian_nonadjournment_sur_achoerodus"));
        //}

        [Fact]
        public async void GetTaxonomiesAsync()
        {
            _mockHttp.When($"{_baseUrl}/taxonomies")
                .WithQueryString("skip=1")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\taxonomies_multiple.json")));

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            var response = await client.GetTaxonomiesAsync(new SkipParameter(1));

            Assert.NotNull(response.ApiUrl);
            Assert.NotEmpty(response.Taxonomies);
        }

        [Fact]
        public async void QueryParameters()
        {

            string url = $"{_baseUrl}/items?elements.personas%5Ball%5D=barista%2Ccoffee%2Cblogger&elements.personas%5Bany%5D=barista%2Ccoffee%2Cblogger&system.sitemap_locations%5Bcontains%5D=cafes&elements.product_name=Hario%20V60&elements.price%5Bgt%5D=1000&elements.price%5Bgte%5D=50&system.type%5Bin%5D=cafe%2Ccoffee&elements.price%5Blt%5D=10&elements.price%5Blte%5D=4&elements.country%5Brange%5D=Guatemala%2CNicaragua&depth=2&elements=price%2Cproduct_name&limit=10&order=elements.price%5Bdesc%5D&skip=2&language=en";
            _mockHttp.When($"{url}")
                .Respond("application/json", " { 'items': [],'modular_content': {},'pagination': {'skip': 2,'limit': 10,'count': 0,'next_page': ''}}");

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            var parameters = new IQueryParameter[]
           {
                new AllFilter("elements.personas", "barista", "coffee", "blogger"),
                new AnyFilter("elements.personas", "barista", "coffee", "blogger"),
                new ContainsFilter("system.sitemap_locations", "cafes"),
                new EqualsFilter("elements.product_name", "Hario V60"),
                new GreaterThanFilter("elements.price", "1000"),
                new GreaterThanOrEqualFilter("elements.price", "50"),
                new InFilter("system.type", "cafe", "coffee"),
                new LessThanFilter("elements.price", "10"),
                new LessThanOrEqualFilter("elements.price", "4"),
                new RangeFilter("elements.country", "Guatemala", "Nicaragua"),
                new DepthParameter(2),
                new ElementsParameter("price", "product_name"),
                new LimitParameter(10),
                new OrderParameter("elements.price", SortOrder.Descending),
                new SkipParameter(2),
                new LanguageParameter("en")
           };

            var response = await client.GetItemsAsync(parameters);

            Assert.Equal(0, response.Items.Count);
        }

        [Fact]
        public async void GetStrongTypesWithLimitedDepth()
        {
            _mockHttp.When($"{_baseUrl}/items/on_roasts")
                .WithQueryString("depth=1")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\on_roasts.json")));

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            // Returns on_roasts content item with related_articles modular element to two other articles.
            // on_roasts
            // |- coffee_processing_techniques
            // |- origins_of_arabica_bourbon
            //   |- on_roasts
            var onRoastsItem = (await client.GetItemAsync<Article>("on_roasts", new DepthParameter(1))).Item;

            Assert.NotNull(onRoastsItem.TeaserImage.First().Description);
            Assert.Equal(2, onRoastsItem.RelatedArticles.Count());
            Assert.Empty(((Article)onRoastsItem.RelatedArticles.First()).RelatedArticles);
            Assert.Empty(((Article)onRoastsItem.RelatedArticles.ElementAt(1)).RelatedArticles);
        }

        [Fact]
        public async void RecursiveModularContent()
        {
            _mockHttp.When($"{_baseUrl}/items/on_roasts")
                .WithQueryString("depth=15")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\onroast_recursive_modular_content.json")));

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            // Try to get recursive modular content on_roasts -> item -> on_roasts
            var article = await client.GetItemAsync<Article>("on_roasts", new DepthParameter(15));

            Assert.NotNull(article.Item);
        }

        [Fact]
        public async void RecursiveInlineModularContent()
        {
            _mockHttp.When($"{_baseUrl}/items/on_roasts")
                .WithQueryString("depth=15")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\onroast_recursive_inline_modular_content.json")));

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            var article = await client.GetItemAsync<Article>("on_roasts", new DepthParameter(15));

            Assert.NotNull(article.Item.BodyCopyRichText);
            Assert.IsType<InlineContentItem>(article.Item.BodyCopyRichText.First());            
        }

        [Fact]
        public void GetStronglyTypedResponse()
        {
            _mockHttp.When($"{_baseUrl}/items/complete_content_item")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\complete_content_item.json")));

            var client = InitializeDeliverClientWithACustomeTypeProvider();

            var response = client.GetItemAsync<CompleteContentItemModel>("complete_content_item").Result;
            CompleteContentItemModel item = response.Item;

            // Assert
            Assert.Equal("Text field value", item.TextField);

            Assert.Equal("<p>Rich text field value</p>", item.RichTextField);

            Assert.Equal(99, item.NumberField);

            Assert.Single(item.MultipleChoiceFieldAsRadioButtons);
            Assert.Equal("Radio button 1", item.MultipleChoiceFieldAsRadioButtons.First().Name);

            Assert.Equal(2, item.MultipleChoiceFieldAsCheckboxes.Count());
            Assert.Equal("Checkbox 1", item.MultipleChoiceFieldAsCheckboxes.First().Name);
            Assert.Equal("Checkbox 2", item.MultipleChoiceFieldAsCheckboxes.ElementAt(1).Name);

            Assert.Equal(new DateTime(2017, 2, 23), item.DateTimeField);

            Assert.Single(item.AssetField);
            Assert.Equal("Fire.jpg", item.AssetField.First().Name);
            Assert.Equal(129170, item.AssetField.First().Size);
            Assert.Equal("https://assets.kenticocloud.com:443/e1167a11-75af-4a08-ad84-0582b463b010/64096741-b658-46ee-b148-b287fe03ea16/Fire.jpg", item.AssetField.First().Url);

            Assert.Single(item.ModularContentField);
            Assert.Equal("Homepage", item.ModularContentField.First().System.Name);

            Assert.Equal(2, item.CompleteTypeTaxonomy.Count());
            Assert.Equal("Option 1", item.CompleteTypeTaxonomy.First().Name);
            Assert.NotNull(response.ApiUrl);
        }

        [Fact]
        public void GetStronglyTypedGenericWithAttributesResponse()
        {
            _mockHttp.When($"{_baseUrl}/items/complete_content_item")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\complete_content_item.json")));

            var httpClient = _mockHttp.ToHttpClient();
            var deliveryOptions = new OptionsWrapper<DeliveryOptions>(new DeliveryOptions { ProjectId = _guid });
            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(deliveryOptions.Value.MaxRetryAttempts));
            var customTypeProvider = A.Fake<ICodeFirstTypeProvider>();
            var codeFirstModelProvider = new CodeFirstModelProvider(null, null, customTypeProvider, new CodeFirstPropertyMapper());
            var client = new DeliveryClient(deliveryOptions, httpClient, null, null, codeFirstModelProvider, resiliencePolicyProvider);


            // Arrange
            A.CallTo(() => customTypeProvider.GetType("complete_content_type")).ReturnsLazily(() => typeof(ContentItemModelWithAttributes));
            A.CallTo(() => customTypeProvider.GetType("homepage")).ReturnsLazily(() => typeof(Homepage));

            ContentItemModelWithAttributes item = (ContentItemModelWithAttributes)client.GetItemAsync<object>("complete_content_item").Result.Item;

            // Assert
            Assert.Equal("Text field value", item.TextFieldWithADifferentName);

            Assert.Equal("<p>Rich text field value</p>", item.RichTextFieldWithADifferentName);

            Assert.Equal(99, item.NumberFieldWithADifferentName);

            Assert.Single(item.MultipleChoiceFieldAsRadioButtonsWithADifferentName);
            Assert.Equal("Radio button 1", item.MultipleChoiceFieldAsRadioButtonsWithADifferentName.First().Name);

            Assert.Equal(2, item.MultipleChoiceFieldAsCheckboxes.Count());
            Assert.Equal("Checkbox 1", item.MultipleChoiceFieldAsCheckboxes.First().Name);
            Assert.Equal("Checkbox 2", item.MultipleChoiceFieldAsCheckboxes.ElementAt(1).Name);

            Assert.Equal(new DateTime(2017, 2, 23), item.DateTimeFieldWithADifferentName);

            Assert.Single(item.AssetFieldWithADifferentName);
            Assert.Equal("Fire.jpg", item.AssetFieldWithADifferentName.First().Name);
            Assert.Equal(129170, item.AssetFieldWithADifferentName.First().Size);
            Assert.Equal("https://assets.kenticocloud.com:443/e1167a11-75af-4a08-ad84-0582b463b010/64096741-b658-46ee-b148-b287fe03ea16/Fire.jpg", item.AssetFieldWithADifferentName.First().Url);

            Assert.Single(item.ModularContentFieldWithADifferentName);
            Assert.Equal("Homepage", ((Homepage)item.ModularContentFieldWithADifferentName.First()).System.Name);
            Assert.Equal("Homepage", ((Homepage)item.ModularContentFieldWithACollectionTypeDefined.First()).System.Name);
            Assert.True(item.ModularContentFieldWithAGenericTypeDefined.First().CallToAction.Length > 0);

            Assert.Equal(2, item.CompleteTypeTaxonomyWithADifferentName.Count());
            Assert.Equal("Option 1", item.CompleteTypeTaxonomyWithADifferentName.First().Name);
        }

        [Fact]
        public void GetStronglyTypedItemsResponse()
        {
            _mockHttp.When($"{_baseUrl}/items")
                .WithQueryString("system.type=complete_content_type")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\complete_content_item_system_type.json")));

            var httpClient = _mockHttp.ToHttpClient();
            var deliveryOptions = new OptionsWrapper<DeliveryOptions>(new DeliveryOptions { ProjectId = _guid });
            var customTypeProvider = A.Fake<ICodeFirstTypeProvider>();
            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(deliveryOptions.Value.MaxRetryAttempts));
            var codeFirstModelProvider = new CodeFirstModelProvider(null, null, customTypeProvider, new CodeFirstPropertyMapper());
            var client = new DeliveryClient(deliveryOptions, httpClient, null, null, codeFirstModelProvider, resiliencePolicyProvider, customTypeProvider);

            // Arrange
            A.CallTo(() => customTypeProvider.GetType("complete_content_type")).ReturnsLazily(() => typeof(ContentItemModelWithAttributes));
            A.CallTo(() => customTypeProvider.GetType("homepage")).ReturnsLazily(() => typeof(Homepage));

            IReadOnlyList<object> items = client.GetItemsAsync<object>(new EqualsFilter("system.type", "complete_content_type")).Result.Items;

            // Assert
            Assert.True(items.All(i => i.GetType() == typeof(ContentItemModelWithAttributes)));
        }

        [Fact]
        public void CastResponse()
        {
            _mockHttp.When($"{_baseUrl}/items/complete_content_item")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\complete_content_item.json")));

            var httpClient = _mockHttp.ToHttpClient();
            var contentLinkUrlResolver = A.Fake<IContentLinkUrlResolver>();
            var deliveryOptions = new OptionsWrapper<DeliveryOptions>(new DeliveryOptions { ProjectId = _guid });
            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(deliveryOptions.Value.MaxRetryAttempts));
            var codeFirstModelProvider = new CodeFirstModelProvider(contentLinkUrlResolver, null, null, new CodeFirstPropertyMapper());
            var client = new DeliveryClient(
                deliveryOptions,
                httpClient,
                contentLinkUrlResolver,
                null,
                codeFirstModelProvider,
                resiliencePolicyProvider
            );

            var response = client.GetItemAsync("complete_content_item").Result;
            var stronglyTypedResponse = response.CastTo<CompleteContentItemModel>();

            // Assert
            Assert.Equal("Text field value", stronglyTypedResponse.Item.TextField);
        }

        [Fact]
        public void CastListingResponse()
        {
            _mockHttp.When($"{_baseUrl}/items")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\items.json")));

            var httpClient = _mockHttp.ToHttpClient();
            var contentLinkUrlResolver = A.Fake<IContentLinkUrlResolver>();
            var deliveryOptions = new OptionsWrapper<DeliveryOptions>(new DeliveryOptions { ProjectId = _guid });
            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(deliveryOptions.Value.MaxRetryAttempts));
            var codeFirstModelProvider = new CodeFirstModelProvider(contentLinkUrlResolver, null, null, new CodeFirstPropertyMapper());
            var client = new DeliveryClient(
                deliveryOptions,
                httpClient,
                contentLinkUrlResolver,
                null,
                codeFirstModelProvider,
                resiliencePolicyProvider
            );

            var response = client.GetItemsAsync().Result;
            var stronglyTypedListingResponse = response.CastTo<CompleteContentItemModel>();

            // Assert
            Assert.NotNull(stronglyTypedListingResponse);
            Assert.True(stronglyTypedListingResponse.Items.Any());
        }

        [Fact]
        public void CastContentItem()
        {
            _mockHttp.When($"{_baseUrl}/items/complete_content_item")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\complete_content_item.json")));

            var httpClient = _mockHttp.ToHttpClient();
            var contentLinkUrlResolver = A.Fake<IContentLinkUrlResolver>();
            var deliveryOptions = new OptionsWrapper<DeliveryOptions>(new DeliveryOptions { ProjectId = _guid });
            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(deliveryOptions.Value.MaxRetryAttempts));
            var codeFirstModelProvider = new CodeFirstModelProvider(contentLinkUrlResolver, null, null, new CodeFirstPropertyMapper());
            var client = new DeliveryClient(
                deliveryOptions,
                httpClient,
                contentLinkUrlResolver,
                null,
                codeFirstModelProvider,
                resiliencePolicyProvider
            );

            var item = client.GetItemAsync("complete_content_item").Result.Item;
            var stronglyTypedResponse = item.CastTo<CompleteContentItemModel>();

            // Assert
            Assert.Equal("Text field value", stronglyTypedResponse.TextField);
        }

        [Fact]
        public void CastContentItems()
        {
            _mockHttp.When($"{_baseUrl}/items")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\items.json")));

            var httpClient = _mockHttp.ToHttpClient();
            var contentLinkUrlResolver = A.Fake<IContentLinkUrlResolver>();
            var deliveryOptions = new OptionsWrapper<DeliveryOptions>(new DeliveryOptions { ProjectId = _guid });
            var codeFirstModelProvider = new CodeFirstModelProvider(contentLinkUrlResolver, null, null, new CodeFirstPropertyMapper());
            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(deliveryOptions.Value.MaxRetryAttempts));
            var client = new DeliveryClient(
                deliveryOptions,
                httpClient,
                contentLinkUrlResolver,
                null,
                codeFirstModelProvider,
                resiliencePolicyProvider
            );

            // Act
            DeliveryItemListingResponse response = client.GetItemsAsync().Result;
            IEnumerable<CompleteContentItemModel> list = response.Items.Where(i => i.System.Type == "complete_content_type").Select(a => a.CastTo<CompleteContentItemModel>());

            // Assert
            Assert.True(list.Any());
        }

        [Fact]
        public void LongUrl()
        {
            _mockHttp.When($"{_baseUrl}/items")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\items.json")));

            var httpClient = _mockHttp.ToHttpClient();
            var deliveryOptions = new OptionsWrapper<DeliveryOptions>(new DeliveryOptions {ProjectId = _guid});
            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(deliveryOptions.Value.MaxRetryAttempts));
            var client = new DeliveryClient(deliveryOptions, httpClient, null, null, null, resiliencePolicyProvider);

            var elements = new ElementsParameter(Enumerable.Range(0, 1000).Select(i => "test").ToArray());
            var inFilter = new InFilter("test", Enumerable.Range(0, 1000).Select(i => "test").ToArray());
            var allFilter = new AllFilter("test", Enumerable.Range(0, 1000).Select(i => "test").ToArray());
            var anyFilter = new AnyFilter("test", Enumerable.Range(0, 1000).Select(i => "test").ToArray());

            // Act
            var response = client.GetItemsAsync(elements, inFilter, allFilter, anyFilter).Result;

            // Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async void TooLongUrlThrows()
        {
            _mockHttp.When($"{_baseUrl}/items")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\items.json")));

            var httpClient = _mockHttp.ToHttpClient();

            var client = new DeliveryClient(new OptionsWrapper<DeliveryOptions>(new DeliveryOptions { ProjectId = _guid }), httpClient);

            var elements = new ElementsParameter(Enumerable.Range(0, 1000000).Select(i => "test").ToArray());

            // Act / Assert
            await Assert.ThrowsAsync<UriFormatException>(async () => await client.GetItemsAsync(elements));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async void PreviewAndSecuredProductionThrowsWhenBothEnabled(bool usePreviewApi, bool useSecuredProduction)
        {
            if (usePreviewApi)
            {
                _mockHttp.When($@"https://preview-deliver.kenticocloud.com/{_guid}/items")
                    .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\items.json")));
            }
            else
            {
                _mockHttp.When($"{_baseUrl}/items")
                    .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\items.json")));
            }

            var httpClient = _mockHttp.ToHttpClient();

            var deliveryOptions = new OptionsWrapper<DeliveryOptions>(new DeliveryOptions
            {
                ProjectId = _guid,
                UsePreviewApi = usePreviewApi,
                UseSecuredProductionApi = useSecuredProduction,
                PreviewApiKey = "someKey",
                SecuredProductionApiKey = "someKey"
            });

            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(deliveryOptions.Value.MaxRetryAttempts));

            var client = new DeliveryClient(deliveryOptions, httpClient, null, null, null, resiliencePolicyProvider);

            if (usePreviewApi && useSecuredProduction)
            {
                // Assert
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.GetItemsAsync());
            }
            else
            {
                var response = await client.GetItemsAsync();

                // Assert
                Assert.NotNull(response);
            }
        }

        [Fact]
        public async void SecuredProductionAddCorrectHeader()
        {
            var securityKey = "someKey";
            var options = new DeliveryOptions
            {
                ProjectId = _guid,
                SecuredProductionApiKey = securityKey,
                UseSecuredProductionApi = true
            };
            _mockHttp.Expect($"{_baseUrl}/items")
                .WithHeaders("Authorization", $"Bearer {securityKey}")
                .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\items.json")));

            var mockHttpClient = _mockHttp.ToHttpClient();
            var deliveryOptions = new OptionsWrapper<DeliveryOptions>(options);
            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => false).RetryAsync(deliveryOptions.Value.MaxRetryAttempts));

            var client = new DeliveryClient(deliveryOptions, mockHttpClient, null, null, null, resiliencePolicyProvider);

            await client.GetItemsAsync();
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async void Retries_WithDefaultSettings_Retries()
        {
            var actualHttpRequestCount = 0;
            var retryAttempts = 4;
            var expectedRetryAttempts = retryAttempts + 1;

            _mockHttp.When($"{_baseUrl}/items")
                .Respond((request) => GetResponseAndLogRequest(HttpStatusCode.RequestTimeout, ref actualHttpRequestCount));
            var httpClient = _mockHttp.ToHttpClient();
            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(retryAttempts));

            var client = new DeliveryClient(new OptionsWrapper<DeliveryOptions>(new DeliveryOptions{ProjectId = _guid}), httpClient, null, null, null, resiliencePolicyProvider);

            await Assert.ThrowsAsync<DeliveryException>(async () => await client.GetItemsAsync());
            Assert.Equal(expectedRetryAttempts, actualHttpRequestCount);
        }

        [Fact]
        public async void Retries_EnableResilienceLogicDisabled_DoesNotRetry()
        {
            var actualHttpRequestCount = 0;

            _mockHttp.When($"{_baseUrl}/items")
                .Respond((request) => GetResponseAndLogRequest(HttpStatusCode.RequestTimeout, ref actualHttpRequestCount));
            var httpClient = _mockHttp.ToHttpClient();

            var deliveryOptions = new DeliveryOptions
            {
                ProjectId = _guid,
                EnableResilienceLogic = false
            };
            var client = new DeliveryClient(new OptionsWrapper<DeliveryOptions>(deliveryOptions), httpClient);

            await Assert.ThrowsAsync<DeliveryException>(async () => await client.GetItemsAsync());
            Assert.Equal(1, actualHttpRequestCount);
        }

        [Fact]
        public async void Retries_WithMaxRetrySet_SettingReflected()
        {
            int retryAttempts = 3;
            int expectedAttempts = retryAttempts + 1;
            int actualHttpRequestCount = 0;

            _mockHttp.When($"{_baseUrl}/items")
                .Respond((request) => GetResponseAndLogRequest(HttpStatusCode.RequestTimeout, ref actualHttpRequestCount));
            var httpClient = _mockHttp.ToHttpClient();
            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(retryAttempts));

            var deliveryOptions = new DeliveryOptions
            {
                ProjectId = _guid,
                MaxRetryAttempts = retryAttempts
            };
            var client = new DeliveryClient(new OptionsWrapper<DeliveryOptions>(deliveryOptions), httpClient, null, null, null, resiliencePolicyProvider);

            await Assert.ThrowsAsync<DeliveryException>(async () => await client.GetItemsAsync());

            Assert.Equal(expectedAttempts, actualHttpRequestCount);
        }

        [Fact]
        public async void Retries_WithCustomResilencePolicy_PolicyUsed()
        {
            int retryAttempts = 1;
            int expectedAttepts = retryAttempts + 1;
            int actualHttpRequestCount = 0;

            _mockHttp.When($"{_baseUrl}/items").Respond((request) => GetResponseAndLogRequest(HttpStatusCode.NotImplemented, ref actualHttpRequestCount));
            var httpClient = _mockHttp.ToHttpClient();

            var mockResilencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => mockResilencePolicyProvider.Policy)
               .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(retryAttempts));

            var client = new DeliveryClient(new OptionsWrapper<DeliveryOptions>(new DeliveryOptions{ProjectId = _guid}), httpClient, null, null, null, mockResilencePolicyProvider);

            await Assert.ThrowsAsync<DeliveryException>(async () => await client.GetItemsAsync());

            A.CallTo(() => mockResilencePolicyProvider.Policy).MustHaveHappened();
            Assert.Equal(expectedAttepts, actualHttpRequestCount);
        }

        [Fact]
        public async void Retries_WithCustomResilencePolicyAndPolicyDisabled_PolicyIgnored()
        {
            int policyRetryAttempts = 2;
            int expectedAttepts = 1;
            int actualHttpRequestCount = 0;

            _mockHttp.When($"{_baseUrl}/items").Respond((request) => GetResponseAndLogRequest(HttpStatusCode.NotImplemented, ref actualHttpRequestCount));
            var httpClient = _mockHttp.ToHttpClient();

            var mockResilencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => mockResilencePolicyProvider.Policy)
               .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(policyRetryAttempts));

            var deliveryOptions = new DeliveryOptions()
            {
                ProjectId = _guid,
                EnableResilienceLogic = false
            };
            var client = new DeliveryClient(new OptionsWrapper<DeliveryOptions>(deliveryOptions), httpClient, null, null, null, mockResilencePolicyProvider);

            await Assert.ThrowsAsync<DeliveryException>(async () => await client.GetItemsAsync());
            Assert.Equal(expectedAttepts, actualHttpRequestCount);
        }

        [Fact]
        public async void Retries_WithCustomResilencePolicyWithMaxRetrySet_PolicyUsedMaxRetryIgnored()
        {
            int policyRetryAttempts = 1;
            int expectedAttepts = policyRetryAttempts + 1;
            int ignoredRetryAttempt = 3;
            int actualHttpRequestCount = 0;

            _mockHttp.When($"{_baseUrl}/items")
                .Respond((request) => GetResponseAndLogRequest(HttpStatusCode.NotImplemented, ref actualHttpRequestCount));
            var httpClient = _mockHttp.ToHttpClient();

            var mockResilencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => mockResilencePolicyProvider.Policy)
               .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(policyRetryAttempts));

            var deliveryOptions = new DeliveryOptions()
            {
                ProjectId = _guid,
                MaxRetryAttempts = ignoredRetryAttempt
            };
            var client = new DeliveryClient(new OptionsWrapper<DeliveryOptions>(deliveryOptions), httpClient, null, null, null, mockResilencePolicyProvider);

            await Assert.ThrowsAsync<DeliveryException>(async () => await client.GetItemsAsync());

            A.CallTo(() => mockResilencePolicyProvider.Policy).MustHaveHappened();
            Assert.Equal(expectedAttepts, actualHttpRequestCount);
        }

        [Fact]
        public async void CorrectSdkVersionHeaderAdded()
        {
            var assembly = typeof(DeliveryClient).Assembly;
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            var sdkVersion = fileVersionInfo.ProductVersion;
            var sdkPackageId = assembly.GetName().Name;

            _mockHttp.Expect($"{_baseUrl}/items")
                    .WithHeaders("X-KC-SDKID", $"nuget.org;{sdkPackageId};{sdkVersion}")
                    .Respond("application/json", File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Fixtures\\DeliveryClient\\items.json")));

            var mockHttpClient = _mockHttp.ToHttpClient();
            var deliveryOptions = new OptionsWrapper<DeliveryOptions>(new DeliveryOptions {ProjectId = _guid});
            var resiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => resiliencePolicyProvider.Policy)
                .Returns(Policy.HandleResult<HttpResponseMessage>(result => false).RetryAsync(deliveryOptions.Value.MaxRetryAttempts));

            var client = new DeliveryClient(deliveryOptions, mockHttpClient, null, null, null, resiliencePolicyProvider);

            await client.GetItemsAsync();

            _mockHttp.VerifyNoOutstandingExpectation();
        }

        private DeliveryClient InitializeDeliverClientWithACustomeTypeProvider()
        {
            var httpClient = _mockHttp.ToHttpClient();
            var deliveryOptions = new OptionsWrapper<DeliveryOptions>(new DeliveryOptions { ProjectId = _guid });
            var contentLinkUrlResolver = A.Fake<IContentLinkUrlResolver>();
            var mockResilencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            A.CallTo(() => mockResilencePolicyProvider.Policy)
               .Returns(Policy.HandleResult<HttpResponseMessage>(result => true).RetryAsync(deliveryOptions.Value.MaxRetryAttempts));
            var codeFirstModelProvider = new CodeFirstModelProvider(contentLinkUrlResolver, null, new CustomTypeProvider(), new CodeFirstPropertyMapper());
            var client = new DeliveryClient(
                deliveryOptions,
                httpClient,
                contentLinkUrlResolver,
                null,
                codeFirstModelProvider,
                mockResilencePolicyProvider,
                new CustomTypeProvider()
            );

            return client;
        }

        private HttpResponseMessage GetResponseAndLogRequest(HttpStatusCode httpStatusCode, ref int actualHttpRequestCount)
        {
            actualHttpRequestCount++;
            return new HttpResponseMessage(httpStatusCode);
        }
    }
}
