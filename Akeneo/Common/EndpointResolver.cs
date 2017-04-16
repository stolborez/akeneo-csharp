﻿using System;
using System.Collections.Concurrent;
using System.Reflection;
using Akeneo.Model;

namespace Akeneo.Common
{
	public class EndpointResolver
	{
		private static readonly Type AttributeBase = typeof(AttributeBase);
		private static readonly Type AttributeOption = typeof(AttributeOption);
		private static readonly Type Family = typeof(Family);
		private static readonly Type Category = typeof(Category);
		private static readonly Type Product = typeof(Product);

		private readonly ConcurrentDictionary<Type, string> _typeToEndpointCache;

		public EndpointResolver()
		{
			_typeToEndpointCache = new ConcurrentDictionary<Type, string>();
		}

		public string ForResourceType<TModel>(string parentCode = null) where TModel : ModelBase
		{
			var isOption = AttributeOption.IsAssignableFrom(typeof(TModel));
			return isOption
				? $"{Endpoints.Attributes}/{parentCode}/options"
				: GetResourceEndpoint(typeof(TModel));
		}

		public string ForResource<TModel>(TModel existing) where TModel : ModelBase
		{
			var baseUri = GetResourceEndpoint(typeof(TModel));
			var product = existing as Product;
			if (product != null)
			{
				return $"{baseUri}/{product.Identifier}";
			}
			var attribute = existing as AttributeBase;
			if (attribute != null)
			{
				return $"{baseUri}/{attribute.Code}";
			}
			var option = existing as AttributeOption;
			if (option != null)
			{
				return $"{baseUri}/{option.Attribute}/option/{option.Code}";
			}
			var family = existing as Family;
			if (family != null)
			{
				return $"{baseUri}/{family.Code}";
			}
			var category= existing as Category;
			if (category != null)
			{
				return $"{baseUri}/{category.Code}";
			}
			return null;
		}

		public string ForResource<TModel>(params string[] code) where TModel : ModelBase
		{
			var formatterStr = GetResourceFormatString(typeof(TModel));
			return string.Format(formatterStr, code);
		}

		protected virtual string GetResourceFormatString(Type modelType)
		{
			var endpoint = GetResourceTypeFormatString(modelType);
			return AttributeOption.IsAssignableFrom(modelType)
				? $"{endpoint}/{{1}}"
				: $"{endpoint}/{{0}}";
		}

		protected virtual string GetResourceTypeFormatString(Type modelType)
		{
			var endpoint = GetResourceEndpoint(modelType);
			return AttributeOption.IsAssignableFrom(modelType)
				? $"{endpoint}/{{0}}/options"
				: $"{endpoint}";
		}

		protected virtual string GetResourceEndpoint(Type modelType)
		{
			return _typeToEndpointCache.GetOrAdd(modelType, type =>
			{
				if (Product.IsAssignableFrom(type))
				{
					return Endpoints.Products;
				}
				if (AttributeBase.IsAssignableFrom(type))
				{
					return Endpoints.Attributes;
				}
				if (Family.IsAssignableFrom(type))
				{
					return Endpoints.Families;
				}
				if (Category.IsAssignableFrom(type))
				{
					return Endpoints.Categories;
				}
				if (AttributeOption.IsAssignableFrom(type))
				{
					return Endpoints.Attributes;
				}
				throw new NotSupportedException($"Unable to find API endpoint for type {modelType.FullName}");
			});
		}

		public string ForPagination<TModel>(int page = 1, int limit = 10, bool withCount = false) where TModel : ModelBase
		{
			var baseUrl = ForResourceType<TModel>();
			return $"{baseUrl}?page={page}&limit={limit}&with_count={withCount.ToString().ToLower()}";
		}

		public string ForPagination<TModel>(string parentCode, int page = 1, int limit = 10, bool withCount = false) where TModel : ModelBase
		{
			var baseUrl = ForResourceType<TModel>();
			return $"{baseUrl}?page={page}&limit={limit}&with_count={withCount.ToString().ToLower()}";
		}
	}
}
