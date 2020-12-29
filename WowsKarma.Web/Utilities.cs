﻿using AngleSharp.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wargaming.WebAPI.Models;
using WowsKarma.Common.Models.DTOs;

namespace WowsKarma.Web
{
	public static class Utilities
	{
		public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
		{
			PropertyNameCaseInsensitive = true,
			NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals
		};

		public static async Task<TResult> DeserializeFromHttpResponseAsync<TResult>(HttpResponseMessage response)
		{
			string json = await response.Content.ReadAsStringAsync();
			TResult result = JsonSerializer.Deserialize<TResult>(json, JsonSerializerOptions);
			return result;
		}

		public static string GetOidcEndpoint(Region region) => region switch
		{
			Region.EU => "https://eu.wargaming.net/id/",
			Region.NA => "https://na.wargaming.net/id/",
			Region.CIS => "https://ru.wargaming.net/id/",
			Region.ASIA => "https://asia.wargaming.net/id/",

			_ => throw new NotImplementedException()
		};

		// Example Identifier:
		// https://eu.wargaming.net/id/0000000-JohnDoe/
		public static AccountListingDTO GetAccountInfoFromOidcUrl(string url)
		{
			Match result = new Regex("([0-9]+)-(\\w+)").Match(url);
			return new(result.Groups[1].Value.ToInteger(uint.MinValue), result.Groups[2].Value);
		}
	}
}
