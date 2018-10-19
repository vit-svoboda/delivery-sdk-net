﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace KenticoCloud.Delivery
{
    internal sealed class DeliveryEndpointUrlBuilder
    {
        private const int UrlMaxLength = 65519;
        private const string UrlTemplateItem = "/items/{0}";
        private const string UrlTemplateItems = "/items";
        private const string UrlTemplateType = "/types/{0}";
        private const string UrlTemplateTypes = "/types";
        private const string UrlTemplateElement = "/types/{0}/elements/{1}";
        private const string UrlTemplateTaxonomy = "/taxonomies/{0}";
        private const string UrlTemplateTaxonomies = "/taxonomies";

        private readonly DeliveryOptions _deliveryOptions;

        public DeliveryEndpointUrlBuilder(DeliveryOptions deliveryOptions)
        {
            _deliveryOptions = deliveryOptions;
        }

        public string GetItemUrl(string codename, string[] parameters)
        {
            return GetUrl(string.Format(UrlTemplateItem, Uri.EscapeDataString(codename)), parameters);
        }

        public string GetItemUrl(string codename, IEnumerable<IQueryParameter> parameters)
        {
            return GetUrl(string.Format(UrlTemplateItem, Uri.EscapeDataString(codename)), parameters);
        }

        public string GetItemsUrl(string[] parameters)
        {
            return GetUrl(UrlTemplateItems, parameters);
        }

        public string GetItemsUrl(IEnumerable<IQueryParameter> parameters)
        {
            return GetUrl(UrlTemplateItems, parameters);
        }

        public string GetTypeUrl(string codename)
        {
            return GetUrl(string.Format(UrlTemplateType, Uri.EscapeDataString(codename)));
        }

        public string GetTypeUrl(string codename, IEnumerable<IQueryParameter> parameters)
        {
            return GetUrl(string.Format(UrlTemplateType, Uri.EscapeDataString(codename)), parameters);
        }

        public string GetTypesUrl(string[] parameters)
        {
            return GetUrl(UrlTemplateTypes, parameters);
        }

        public string GetTypesUrl(IEnumerable<IQueryParameter> parameters)
        {
            return GetUrl(UrlTemplateTypes, parameters);
        }

        public string GetContentElementUrl(string contentTypeCodename, string contentElementCodename)
        {
            return GetUrl(string.Format(UrlTemplateElement, Uri.EscapeDataString(contentTypeCodename), Uri.EscapeDataString(contentElementCodename)));
        }

        public string GetTaxonomyUrl(string codename)
        {
            return GetUrl(string.Format(UrlTemplateTaxonomy, Uri.EscapeDataString(codename)));
        }

        public string GetTaxonomiesUrl(string[] parameters)
        {
            return GetUrl(UrlTemplateTaxonomies, parameters);
        }

        public string GetTaxonomiesUrl(IEnumerable<IQueryParameter> parameters)
        {
            return GetUrl(UrlTemplateTaxonomies, parameters);
        }

        private string GetUrl(string path, IEnumerable<IQueryParameter> parameters)
        {
            if (parameters != null && parameters.Any())
            {
                return GetUrl(path, parameters.Select(parameter => parameter.GetQueryStringParameter()).ToArray());
            }

            return GetUrl(path);
        }

        private string GetUrl(string path, string[] parameters = null)
        {
            var hostUrl = AssembleHost();
            var url = AssembleUrl(path, parameters, hostUrl);

            if (url.Length > UrlMaxLength)
            {
                throw new UriFormatException("The request url is too long. Split your query into multiple calls.");
            }

            return url;
        }

        private static string AssembleUrl(string path, string[] parameters, string hostUrl)
        {
            var urlBuilder = new UriBuilder(hostUrl + path);

            if (parameters != null && parameters.Length > 0)
            {
                urlBuilder.Query = string.Join("&", parameters);
            }

            var url = urlBuilder.ToString();
            return url;
        }

        private string AssembleHost()
        {
            var endpointUrlTemplate = _deliveryOptions.UsePreviewApi
                            ? _deliveryOptions.PreviewEndpoint
                            : _deliveryOptions.ProductionEndpoint;
            var projectId = Uri.EscapeDataString(_deliveryOptions.ProjectId);
            var hostUrl = string.Format(endpointUrlTemplate, projectId);
            return hostUrl;
        }
    }
}
