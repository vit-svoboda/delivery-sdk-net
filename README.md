# Kentico Cloud Delivery .NET SDK

[![Build status](https://ci.appveyor.com/api/projects/status/3m3q2ads2y43bh9o/branch/master?svg=true)](https://ci.appveyor.com/project/kentico/deliver-net-sdk/branch/master)
[![Forums](https://img.shields.io/badge/chat-on%20forums-orange.svg)](https://forums.kenticocloud.com)

| Paradigm        | Package  | Downloads | Documentation |
| ------------- |:-------------:| :-------------:|  :-------------:|
| Async         | [![NuGet](https://img.shields.io/nuget/v/KenticoCloud.Delivery.svg)](https://www.nuget.org/packages/KenticoCloud.Delivery) | [![NuGet](https://img.shields.io/nuget/dt/kenticocloud.delivery.svg)](https://www.nuget.org/packages/KenticoCloud.Delivery) | [📖](#using-the-deliveryclient) |
| Reactive      | [![NuGet](https://img.shields.io/nuget/v/KenticoCloud.Delivery.Rx.svg)](https://www.nuget.org/packages/KenticoCloud.Delivery.Rx) | [![NuGet](https://img.shields.io/nuget/dt/kenticocloud.delivery.Rx.svg)](https://www.nuget.org/packages/KenticoCloud.Delivery.Rx) | [📖](#using-the-kenticoclouddeliveryrx-reactive-library) |

## Summary

The Kentico Cloud Delivery .NET SDK is a client library used for retrieving content from Kentico Cloud.

You can use it via any of the following NuGet packages:

* [KenticoCloud.Delivery](https://www.nuget.org/packages/KenticoCloud.Delivery)
* [KenticoCloud.Delivery.Rx](https://www.nuget.org/packages/KenticoCloud.Delivery.Rx)

The first package provides the [DeliveryClient](#using-the-deliveryclient) object to consume Kentico Cloud data via the traditional async way. The second one provides the [DeliveryObservableProxy](#using-the-kenticoclouddeliveryrx-reactive-library) object that enables the reactive way of consuming the data.

The SDK targets the [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard), which means it can be used in .NET Framework 4.6.1 projects and above, and .NET Core 2.0 projects and above.

## Using the DeliveryClient

The `DeliveryClient` class is the main class of the SDK. Using this class, you can retrieve content from your Kentico Cloud projects.

To create an instance of the class, you need to provide a [project ID](https://developer.kenticocloud.com/v1/docs/getting-content#section-getting-content-items).

```csharp
// Initializes an instance of the DeliveryClient client by building it with DeliveryClientBuilder class
IDeliveryClient client = DeliveryClientBuilder.WithProjectId("YOUR_PROJECT_ID").Build();
```

You can also provide the project ID and other parameters by passing a function, that returns [`DeliveryOptions`](https://github.com/Kentico/delivery-sdk-net/blob/master/KenticoCloud.Delivery/Configuration/DeliveryOptions%20.cs) object, to the `DeliveryClientBuilder.WithOptions` method. It is recommended to create the `DeliveryOptions` instance using the `DeliveryOptionsBuilder` class. It is used to set the following parameters:

* `ProjectId` – sets the project identifier.
* `PreviewApiKey` – sets the Delivery Preview API key. See [previewing unpublished content](#previewing-unpublished-content).
* `UsePreviewApi` – determines whether to use the Delivery Preview API. See [previewing unpublished content](#previewing-unpublished-content).
* `SecuredProductionApiKey` – sets the production Delivery API key
* `UseSecuredProductionApi` – determines whether to authenticate against the production Delivery API with an API key.
* `EnableResilienceLogic` – determines whether HTTP requests will use a retry logic.
* `WaitForLoadingNewContent` – makes the client instance wait while fetching updated content, useful when acting upon [webhook calls](https://developer.kenticocloud.com/docs/webhooks).
* `MaxRetryAttempts` – sets the maximum retry attempts.
* `ProductionEndpoint` – sets a custom production endpoint address.
* `PreviewEndpoint` – sets a custom preview endpoint address.

```csharp
DeliveryClientBuilder
	.WithOptions(builder =>
		builder
			.WithProjectId("YOUR_PROJECT_ID")
			.UseProductionApi
			.WithMaxRetryAttempts(maxRetryAttempts)
			.Build())
	.Build();
```

For advanced configuration options using Dependency Injection and ASP.NET Core Configuration API, see the SDK's [wiki](https://github.com/Kentico/delivery-sdk-net/wiki/Using-the-ASP.NET-Core-Configuration-API-and-DI-to-Instantiate-the-DeliveryClient).

## Basic querying

Once you have a `DeliveryClient` instance, you can start querying your project repository by calling methods on the instance.

```csharp
// Retrieves a single content item
DeliveryItemResponse response = await client.GetItemAsync("about_us");

// Retrieves a list of all content items
DeliveryItemListingResponse listingResponse = await client.GetItemsAsync();
```

### Filtering retrieved data

The SDK supports full scale of the API querying and filtering capabilities as described in the [API reference](https://developer.kenticocloud.com/reference#content-filtering).

```csharp
// Retrieves a list of the specified elements from the first 10 content items of
// the 'brewer' content type, ordered by the 'product_name' element value
DeliveryItemListingResponse response = await client.GetItemsAsync(
    new EqualsFilter("system.type", "brewer"),
    new ElementsParameter("image", "price", "product_status", "processing"),
    new LimitParameter(10),
    new OrderParameter("elements.product_name")
);
```

### Getting localized items

The language selection is just a matter of specifying one additional filtering parameter to the query.

```csharp
// Retrieves a list of the specified elements from the first 10 content items of
// the 'brewer' content type, ordered by the 'product_name' element value
DeliveryItemListingResponse response = await client.GetItemsAsync(
    new LanguageParameter("es-ES"),
    new EqualsFilter("system.type", "brewer"),
    new ElementsParameter("image", "price", "product_status", "processing"),
    new LimitParameter(10),
    new OrderParameter("elements.product_name")
);
```

### Strongly-typed responses

The `DeliveryClient` also supports retrieving of strongly-typed models.

```csharp
// Retrieving a single content item
DeliveryItemResponse<Article> response = await client.GetItemAsync<Article>("latest_article");

// Retrieving all content items
DeliveryItemListingResponse<Article> listingResponse = await client.GetItemsAsync<Article>();
```

See [Working with Strongly Typed Models](https://github.com/Kentico/delivery-sdk-net/wiki/Working-with-Strongly-Typed-Models-(aka-Code-First-Approach)) in the wiki to learn how to generate models and adjust the logic to your needs.

## Previewing unpublished content

To retrieve unpublished content, you need to create a `DeliveryClient` with both Project ID and Preview API key. Each Kentico Cloud project has its own Preview API key.

```csharp
// Note: Within a single project, we recommend that you work with only
// either the production or preview Delivery API, not both.
DeliveryClient client = DeliveryClientBuilder.WithOptions(builder => builder.WithProjectId("YOUR_PROJECT_ID").UsePreviewApi("YOUR_PREVIEW_API_KEY"));
```

For more details, see [Previewing unpublished content using the Delivery API](https://developer.kenticocloud.com/docs/preview-content-via-api).

## Response structure

For full description of single and multiple content item JSON response formats, see our [API reference](https://developer.kenticocloud.com/reference#response-structure).

### Single content item response

When retrieving a single content item, you get an instance of the `DeliveryItemResponse` class. This class represents the JSON response from the Delivery API endpoint and contains the requested `ContentItem` as a property.

### Multiple content items response

When retrieving a list of content items, you get an instance of the `DeliveryItemListingResponse`. This class represents the JSON response from the Delivery API endpoint and contains:

* `Pagination` property with information about the following:
  * `Skip`: requested number of content items to skip
  * `Limit`: requested page size
  * `Count`: the total number of retrieved content items
  * `NextPageUrl`: the URL of the next page
* A list of the requested content items

### ContentItem structure

The `ContentItem` class provides the following:

* `System` property with metadata such as code name, display name, type, or sitemap location.
* `Elements` as a dynamically typed property containing all the elements included in the response structured by code names.
* Methods for easier access to certain types of content elements such as modular content, or assets.

## Getting content item properties

You can access information about a content item (i.e., its ID, codename, name, location in sitemap, date of last modification, and its content type codename) by using the `System` property.

```csharp
// Retrieves name of an article content item
articleItem.System.Name

// Retrieves codename of an article content item
articleItem.System.Codename

// Retrieves name of the content type of an article content item
articleItem.System.Type
```

## Getting element values

The SDK provides methods for retrieving content from content elements such as Asset, Text, Rich Text, Multiple choice, etc.

### Text and Rich text

For text elements, you can use the `GetString` method.

```csharp
// Retrieves an article text from the 'body_copy' Text element
articleItem.GetString("body_copy")
```

The Rich text element can contain links to other content items within your project. See [Resolving links to content items](https://github.com/Kentico/delivery-sdk-net/wiki/Resolving-Links-to-Content-Items) for more details.

### Asset

```csharp
// Retrieves a teaser image URL
articleItem.GetAssets("teaser_image").First().Url
```

### Multiple choice

To get a list of options defined in a Multiple choice content element, you first need to retrieve the content element itself. For this purpose, you can use the `GetContentElementAsync` method, which takes the codename of a content type and the codename of a content element.

```csharp
// Retrieves the 'processing' element of the 'coffee' content type
ContentElement element = await client.GetContentElementAsync("coffee", "processing");
```

After you retrieve the Multiple choice element, you can work with its list of options. Each option has the following properties:


Property | Description | Example
---------|----------|---------
 Name | The display name of the option. | `Dry (Natural)`
 Codename | The codename of the option. | `dry__natural_`

To put the element's options in a list, you can use the following code:

```csharp
List<SelectListItem> items = new List<SelectListItem>();

foreach (var option in element.Options)
{
    items.Add(new SelectListItem {
        Text = option.Name,
        Value = option.Codename,
        Selected = (option.Codename == "semi_dry")
    });
}
```

### Modular content

```csharp
// Retrieves related articles
articleItem.GetModularContent("related_articles")
```

## Using the Image transformations
The [ImageUrlBuilder class](https://github.com/Kentico/delivery-sdk-net/blob/master/KenticoCloud.Delivery/ImageTransformation/ImageUrlBuilder.cs) exposes methods for applying image transformations on the Asset URL.

```csharp
string assetUrl = articleItem.GetAssets("teaser_image").First().Url;
ImageUrlBuilder builder = new ImageUrlBuilder(assetUrl);
string transformedAssetUrl = builder.WithFocalPointCrop(560, 515, 2)
                                    .WithDPR(3)
                                    .WithAutomaticFormat(ImageFormat.Png)
                                    .WithCompression(ImageCompression.Lossy)
                                    .WithQuality(85)
                                    .Url;
```

For list of supported transformations and more information visit the Kentico Delivery API reference at <https://developer.kenticocloud.com/v1/reference?#image-transformation>.

## Resilience capabilities
By default, the SDK uses a retry logic (policy) thanks to `DeliveryOptions.EnableResilienceLogic` being set to `true`. It can be disabled. The default policy retries the HTTP requests if the following status codes are returned:

* `RequestTimeout`
* `InternalServerError`
* `BadGateway`
* `ServiceUnavailable`
* `GatewayTimeout`

The default policy retries requests for 5 times, totalling to 6 overall attempts, before a `DeliveryException` is thrown. The number of attempts can be configured via `DeliveryOptions.MaxRetryAttempts`. The consecutive attempts are delayed in an exponential way, i.e. after 2<sup>2</sup> * 100 milliseconds, 2<sup>3</sup> * 100 milliseconds and so on.

The default resilience policy is implemented using the [Polly](https://github.com/App-vNext/Polly) library. You can also implement your own Polly policy wrapped in your own `IResiliencePolicyProvider` instance and plug it as an optional parameter into the constructor of `DeliveryClient` or by the `ResiliencePolicyProvider` property anytime.

## Using the KenticoCloud.Delivery.Rx reactive library

The [DeliveryObservableProxy class](https://github.com/Kentico/delivery-sdk-net/blob/master/KenticoCloud.Delivery.Rx/DeliveryObservableProxy.cs) provides a reactive way of retrieving Kentico Cloud content.

The `DeliveryObservableProxy` class constructor accepts an [IDeliveryClient](https://github.com/Kentico/delivery-sdk-net/blob/master/KenticoCloud.Delivery/IDeliveryClient.cs) instance, therefore you are free to create the `DeliveryClient` (or its derivatives) in any of [the available ways](#using-the-deliveryclient).

```csharp
public IDeliveryClient DeliveryClient => new DeliveryClient("975bf280-fd91-488c-994c-2f04416e5ee3");
public DeliveryObservableProxy DeliveryObservableProxy => new DeliveryObservableProxy(DeliveryClient);
```

The `DeliveryObservableProxy` class exposes methods that mirror the public methods of the [DeliveryClient](https://github.com/Kentico/delivery-sdk-net/blob/master/KenticoCloud.Delivery/DeliveryClient.cs). The methods have the same names, with an `Observable` suffix. They call the `DeliveryClient` methods in the background.

```csharp
IObservable<Article> articlesWithBaristaPersona =
	DeliveryObservableProxy.GetItemsObservable<Article>(new ContainsFilter("elements.personas", "barista"));
```

Unlike most of the `DeliveryClient` methods that return data wrapped in `Delivery*Response` objects, their `*Observable` counterparts always return sequences of the Kentico Cloud artifacts themselves (not wrapped). Should an error response be returned by the `DeliveryClient`, the observable sequence will terminate with the conventional [OnError](https://docs.microsoft.com/en-us/dotnet/api/system.iobserver-1.onerror) call.

## How to use [SourceLink](https://github.com/dotnet/sourcelink/) for debugging

This repository is configured to generate a SourceLink tag in the NuGet package that allows debugging this repository's source code when it is referenced as a Nuget package. The source code is downloaded directly from GitHub to Visual Studio.

### How to configure SourceLink
1. Open a solution with a project referencing the KenticoCloud.Delivery (or KenticoCloud.Delivery.RX) Nuget package
2. Open Tools -> Options -> Debugging -> General
    * Clear **Enable Just My Code**
    * Select **Enable Source Link Support**
    * (Optional) Clear **Require source files to exactly match the original version**
3. Build your solution
4. Copy the PDB files from the KenticoCloud.Delivery NuGet package into the bin folder next to your own project PDB files*
5. Run a debugging session and try to step into the KenticoCloud.Delivery code
6. Allow Visual Studio to download the source code from GitHub
  * ![SourceLink confirmation dialog](/.github/assets/allow_sourcelink_download.png)

**Now you are able to debug the source code of our library without needing to download the source code manually!**

\* Currently the SymbolSource.org server [does not allow hosting of portable PDB files](https://github.com/SymbolSource/SymbolSource/issues/7) and NuGet still [has not come up with a best practice solution](https://github.com/NuGet/Home/issues/6104). That is why we recommend to copy PDB files manually instead of using a [Symbol Server](https://github.com/dotnet/designs/blob/master/accepted/diagnostics/debugging-with-symbols-and-sources.md).

## Further information

For more developer resources, visit the Kentico Cloud Developer Hub at <https://developer.kenticocloud.com>.

### Building the sources

Prerequisites:

**Required:**
[.NET Core SDK](https://www.microsoft.com/net/download/core).

Optional:
* [Visual Studio 2017](https://www.visualstudio.com/vs/) for full experience
* or [Visual Studio Code](https://code.visualstudio.com/)

## Feedback & Contributing

Check out the [contributing](https://github.com/Kentico/delivery-sdk-net/blob/master/CONTRIBUTING.md) page to see the best places to file issues, start discussions, and begin contributing.

### Wall of Fame
We would like to express our thanks to the following people who contributed and made the project possible:

- [Jarosław Jarnot](https://github.com/jjarnot-vimanet) - [Vimanet](http://vimanet.com)
- [Varinder Singh](https://github.com/VarinderS) - [Kudos Web](http://www.kudosweb.com)
- [Charith Sooriyaarachchi](https://github.com/charithsoori) - [99X Technology](http://www.99xtechnology.com/)

Would you like to become a hero too? Pick an [issue](https://github.com/Kentico/delivery-sdk-net/issues) and send us a pull request!

![Analytics](https://kentico-ga-beacon.azurewebsites.net/api/UA-69014260-4/Kentico/delivery-sdk-net?pixel)
